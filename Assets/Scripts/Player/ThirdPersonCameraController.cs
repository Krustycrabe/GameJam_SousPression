using UnityEngine;

/// <summary>Caméra troisième personne — smoothing frame-rate indépendant via SmoothDamp.</summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private float _distance = 4f;
    [SerializeField] private float _sensitivity = 3f;
    [SerializeField] private float _verticalMin = -20f;
    [SerializeField] private float _verticalMax = 60f;
    [SerializeField] private float _followSmoothing = 12f;  // plus élevé = suit plus près
    [SerializeField] private float _followSmoothingY = 6f;
    [SerializeField] private float _rotationSmoothing = 8f;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.6f, 0f);

    [Header("Visée")]
    [SerializeField] private float _aimDistance = 2f;
    [SerializeField] private float _aimTransitionSpeed = 8f;

    private Transform _target;
    private Vector3 _pivotPosition;
    private Vector3 _pivotVelocity;   // utilisé par SmoothDamp

    private float _targetYaw;
    private float _targetPitch;
    private float _smoothedYaw;
    private float _smoothedPitch;
    private float _currentDistance;
    private bool _isAiming;
    private bool _isInitialized;

    private void OnEnable()
    {
        PlayerEvents.OnPlayerTransformReady += HandlePlayerReady;
        PlayerEvents.OnLookInput += HandleLookInput;
        PlayerEvents.OnAimChanged += HandleAimChanged;
    }

    private void OnDisable()
    {
        PlayerEvents.OnPlayerTransformReady -= HandlePlayerReady;
        PlayerEvents.OnLookInput -= HandleLookInput;
        PlayerEvents.OnAimChanged -= HandleAimChanged;
    }

    private void HandleAimChanged(bool isAiming) => _isAiming = isAiming;

    private void HandlePlayerReady(Transform playerTransform)
    {
        _target = playerTransform;
        _pivotPosition = _target.position + _offset;
        _currentDistance = _distance;
        _smoothedYaw = _targetYaw;
        _smoothedPitch = _targetPitch;
        _isInitialized = true;
    }

    private void HandleLookInput(Vector2 delta)
    {
        _targetYaw += delta.x * _sensitivity;
        _targetPitch -= delta.y * _sensitivity;
        _targetPitch = Mathf.Clamp(_targetPitch, _verticalMin, _verticalMax);
    }

    private void LateUpdate()
    {
        if (!_isInitialized) return;

        // Distance visée
        _currentDistance = Mathf.Lerp(
            _currentDistance,
            _isAiming ? _aimDistance : _distance,
            1f - Mathf.Exp(-_aimTransitionSpeed * Time.deltaTime)
        );

        // Rotation — formule exponentielle frame-rate indépendante
        float rotT = 1f - Mathf.Exp(-_rotationSmoothing * Time.deltaTime);
        _smoothedYaw = Mathf.LerpAngle(_smoothedYaw, _targetYaw, rotT);
        _smoothedPitch = Mathf.LerpAngle(_smoothedPitch, _targetPitch, rotT);

        // Pivot XZ/Y — SmoothDamp : frame-rate indépendant, sans overshooting
        Vector3 targetPoint = _target.position + _offset;
        float smoothTimeXZ = 1f / _followSmoothing;
        float smoothTimeY = 1f / _followSmoothingY;

        _pivotPosition = new Vector3(
            Mathf.SmoothDamp(_pivotPosition.x, targetPoint.x, ref _pivotVelocity.x, smoothTimeXZ),
            Mathf.SmoothDamp(_pivotPosition.y, targetPoint.y, ref _pivotVelocity.y, smoothTimeY),
            Mathf.SmoothDamp(_pivotPosition.z, targetPoint.z, ref _pivotVelocity.z, smoothTimeXZ)
        );

        // Position et orientation finale
        Quaternion rotation = Quaternion.Euler(_smoothedPitch, _smoothedYaw, 0f);
        transform.position = _pivotPosition + rotation * new Vector3(0f, 0f, -_currentDistance); // ← fix bug
        transform.LookAt(_pivotPosition);
    }
}
