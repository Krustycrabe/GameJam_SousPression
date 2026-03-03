using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    private const float Gravity = -9.81f;
    private const float RaycastInset = 0.1f;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravityScale = 2f;
    [SerializeField] private float _fallBlendDistance = 1f;
    [SerializeField] private LayerMask _groundMask = ~0;

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private float _verticalVelocity;
    private bool _wasGrounded;

    private void Awake() => _characterController = GetComponent<CharacterController>();
    private void Start() => PlayerEvents.RaisePlayerTransformReady(transform);

    private bool _isClimbing;

    private void OnEnable()
    {
        PlayerEvents.OnMoveInput += HandleMoveInput;
        PlayerEvents.OnJumpForceRequested += HandleJumpForce;
        PlayerEvents.OnClimbStarted += HandleClimbStarted;
        PlayerEvents.OnClimbCompleted += HandleClimbCompleted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnMoveInput -= HandleMoveInput;
        PlayerEvents.OnJumpForceRequested -= HandleJumpForce;
        PlayerEvents.OnClimbStarted -= HandleClimbStarted;
        PlayerEvents.OnClimbCompleted -= HandleClimbCompleted;
    }

    private void HandleClimbStarted() => _isClimbing = true;
    private void HandleClimbCompleted()
    {
        _isClimbing = false;
        _verticalVelocity = -2f; // ← reset : évite que la vélocité négative accumulée pousse dans le sol
    }

    private void Update()
    {
        if (_isClimbing) return; // ← toute la physique est suspendue pendant la grimpe
        ApplyGravity();
        Move();
        NotifyState();
    }


    private void HandleMoveInput(Vector2 input) => _moveInput = input;
    private void HandleJumpForce(float force) => _verticalVelocity = force;

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += Gravity * _gravityScale * Time.deltaTime;
    }

    private void Move()
    {
        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 horizontalMove = (forward * _moveInput.y + right * _moveInput.x) * _moveSpeed;
        Vector3 finalMove = horizontalMove + Vector3.up * _verticalVelocity;

        _characterController.Move(finalMove * Time.deltaTime);

        if (horizontalMove.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalMove.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }

    private void NotifyState()
    {
        bool isGrounded = _characterController.isGrounded;

        float normalizedSpeed = Mathf.Clamp01(new Vector2(_moveInput.x, _moveInput.y).magnitude);
        PlayerEvents.RaiseSpeedChanged(normalizedSpeed);

        if (isGrounded != _wasGrounded)
        {
            PlayerEvents.RaiseGroundedChanged(isGrounded);

            if (isGrounded)
                PlayerEvents.RaiseFallBlendChanged(0f);

            _wasGrounded = isGrounded;
        }

        if (!isGrounded)
            PlayerEvents.RaiseFallBlendChanged(ComputeFallBlend());
    }

    /// <summary>Retourne 0 si loin du sol (Falling Idle), 1 si proche (Landing).</summary>
    private float ComputeFallBlend()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * RaycastInset;
        float maxRayLength = _fallBlendDistance + RaycastInset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxRayLength, _groundMask, QueryTriggerInteraction.Ignore))
        {
            float distanceFromFeet = Mathf.Max(0f, hit.distance - RaycastInset);
            return 1f - Mathf.Clamp01(distanceFromFeet / _fallBlendDistance);
        }

        return 0f;
    }
}
