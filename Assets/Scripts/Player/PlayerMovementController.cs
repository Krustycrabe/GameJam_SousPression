using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    private const float Gravity = -9.81f;
    private const float RaycastInset = 0.1f;
    private const float LaunchDecay = 3f;

    [Header("Déplacement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravityScale = 2f;
    [SerializeField] private float _fallBlendDistance = 1f;
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _aimSpeedMultiplier = 0.5f;

    [Header("Glissade")]
    [SerializeField] private float _slipAcceleration = 10f;

    [Header("Sweep Fall Slide")]
    [SerializeField] private float _sweepSlideSpeed = 3f;
    [SerializeField] private float _sweepSlideDecay = 1.2f;

    private CharacterController _characterController;
    private Camera _camera;

    private Vector2 _moveInput;
    private float _verticalVelocity;
    private bool _wasGrounded;
    private bool _isClimbing;
    private bool _isSweepFall;
    private bool _isAiming;
    private bool _isSlipping;
    private Vector3 _launchVelocity;
    private Vector3 _currentHorizontalVelocity;
    private float _activeAcceleration;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;
        _activeAcceleration = _slipAcceleration;
    }

    private void Start() => PlayerEvents.RaisePlayerTransformReady(transform);

    private void OnEnable()
    {
        PlayerEvents.OnMoveInput += HandleMoveInput;
        PlayerEvents.OnJumpForceRequested += HandleJumpForce;
        PlayerEvents.OnClimbStarted += HandleClimbStarted;
        PlayerEvents.OnClimbCompleted += HandleClimbCompleted;
        PlayerEvents.OnAimChanged += HandleAimChanged;
        PlayerEvents.OnTrampolineBounce += HandleTrampolineBounce;
        PlayerEvents.OnSweepFallStarted += HandleSweepFallStarted;
        PlayerEvents.OnSweepFallCompleted += HandleSweepFallCompleted;
        PlayerEvents.OnSlipChanged += HandleSlipChanged;
        PlayerEvents.OnSlipReset += HandleSlipReset;
    }

    private void OnDisable()
    {
        PlayerEvents.OnMoveInput -= HandleMoveInput;
        PlayerEvents.OnJumpForceRequested -= HandleJumpForce;
        PlayerEvents.OnClimbStarted -= HandleClimbStarted;
        PlayerEvents.OnClimbCompleted -= HandleClimbCompleted;
        PlayerEvents.OnAimChanged -= HandleAimChanged;
        PlayerEvents.OnTrampolineBounce -= HandleTrampolineBounce;
        PlayerEvents.OnSweepFallStarted -= HandleSweepFallStarted;
        PlayerEvents.OnSweepFallCompleted -= HandleSweepFallCompleted;
        PlayerEvents.OnSlipChanged -= HandleSlipChanged;
        PlayerEvents.OnSlipReset -= HandleSlipReset;
    }

    private void HandleMoveInput(Vector2 input) => _moveInput = input;
    private void HandleJumpForce(float force) => _verticalVelocity = force;
    private void HandleAimChanged(bool isAiming) => _isAiming = isAiming;
    private void HandleClimbStarted() => _isClimbing = true;
    private void HandleSlipReset() => _currentHorizontalVelocity = Vector3.zero;

    private void HandleSlipChanged(float acceleration, bool isSlipping)
    {
        _activeAcceleration = acceleration;
        _isSlipping = isSlipping;
    }

    private void HandleClimbCompleted()
    {
        _isClimbing = false;
        _verticalVelocity = -2f;
    }

    /// <summary>
    /// Donne une impulsion initiale dans la direction de marche courante,
    /// puis laisse la vélocité décélérer naturellement pendant l'animation.
    /// </summary>
    private void HandleSweepFallStarted()
    {
        _isSweepFall = true;

        Vector3 slideDir = _currentHorizontalVelocity.sqrMagnitude > 0.1f
            ? _currentHorizontalVelocity.normalized
            : transform.forward;

        _currentHorizontalVelocity = slideDir * _sweepSlideSpeed;
    }

    private void HandleSweepFallCompleted()
    {
        _isSweepFall = false;
        _verticalVelocity = -2f;
        _currentHorizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// Lance le joueur dans la direction de son input courant.
    /// Sans input → uniquement la composante verticale (rebond neutre).
    /// </summary>
    private void HandleTrampolineBounce(float vertical, float horizontal)
    {
        _verticalVelocity = vertical;

        Vector3 forward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
        Vector3 inputDir = forward * _moveInput.y + right * _moveInput.x;

        _launchVelocity = inputDir.magnitude > 0.1f
            ? inputDir.normalized * horizontal
            : Vector3.zero;

        PlayerEvents.RaiseJumpExecuted();
    }

    private void Update()
    {
        if (_isClimbing) return;

        ApplyGravity();

        if (_isSweepFall)
        {
            _currentHorizontalVelocity = Vector3.MoveTowards(
                _currentHorizontalVelocity,
                Vector3.zero,
                _sweepSlideDecay * Time.deltaTime
            );

            _characterController.Move(
                (_currentHorizontalVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime
            );

            NotifyState();
            return;
        }

        Move();
        NotifyState();
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += Gravity * _gravityScale * Time.deltaTime;
    }

    private void Move()
    {
        _launchVelocity = Vector3.MoveTowards(_launchVelocity, Vector3.zero, LaunchDecay * Time.deltaTime);

        Vector3 forward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
        float speed = _moveSpeed * (_isAiming ? _aimSpeedMultiplier : 1f);
        Vector3 targetMove = (forward * _moveInput.y + right * _moveInput.x) * speed;

        // Hors zone : réactif immédiat. En zone glissante : MoveTowards avec inertie.
        _currentHorizontalVelocity = _isSlipping
            ? Vector3.MoveTowards(_currentHorizontalVelocity, targetMove, _activeAcceleration * Time.deltaTime)
            : targetMove;

        Vector3 horizontalMove = _currentHorizontalVelocity + _launchVelocity;
        _characterController.Move((horizontalMove + Vector3.up * _verticalVelocity) * Time.deltaTime);

        // Rotation sur targetMove (intention input) — réactive même en glissade
        Vector3 rotationDir = _launchVelocity.sqrMagnitude > 0.5f ? _launchVelocity : targetMove;
        if (_isAiming)
        {
            Quaternion target = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 15f * Time.deltaTime);
        }
        else if (rotationDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rotationDir.normalized), 15f * Time.deltaTime);
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
            if (isGrounded) PlayerEvents.RaiseFallBlendChanged(0f);
            _wasGrounded = isGrounded;
        }

        if (!isGrounded) PlayerEvents.RaiseFallBlendChanged(ComputeFallBlend());
    }

    /// <summary>Retourne 0 si loin du sol (Falling), 1 si proche (Landing).</summary>
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
