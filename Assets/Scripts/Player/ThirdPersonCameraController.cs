using UnityEngine;

/// <summary>Caméra troisième personne sur bras rigide avec smoothing de rotation indépendant.</summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private float _distance = 4f;
    [SerializeField] private float _sensitivity = 3f;
    [SerializeField] private float _verticalMin = -20f;
    [SerializeField] private float _verticalMax = 60f;
    [SerializeField] private float _followSmoothing = 10f;
    [SerializeField] private float _rotationSmoothing = 8f;   // 3 = très doux, 20 = quasi-immédiat
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.6f, 0f);

    [Header("Visée")]
    [SerializeField] private float _aimDistance = 2f;
    [SerializeField] private float _aimTransitionSpeed = 8f;

    private float _currentDistance;
    private bool _isAiming;


    private Transform _target;
    private Vector3 _pivotPosition;

    // Angles cibles : accumulent le delta brut de la souris
    private float _targetYaw;
    private float _targetPitch;

    // Angles smoothés : lerpent vers les cibles, pilotent la caméra
    private float _smoothedYaw;
    private float _smoothedPitch;

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

        // Les angles smoothés sont initialisés sur les cibles pour éviter
        // un lerp depuis 0° au premier frame
        _smoothedYaw = _targetYaw;
        _smoothedPitch = _targetPitch;

        _isInitialized = true;
    }

    private void HandleLookInput(Vector2 delta)
    {
        // Accumulation brute — pas de smoothing ici
        _targetYaw += delta.x * _sensitivity;
        _targetPitch -= delta.y * _sensitivity;
        _targetPitch = Mathf.Clamp(_targetPitch, _verticalMin, _verticalMax);
    }

    private void LateUpdate()
    {
        if (!_isInitialized) return;

        _currentDistance = Mathf.Lerp(_currentDistance, _isAiming ? _aimDistance : _distance, _aimTransitionSpeed * Time.deltaTime);

        _smoothedYaw = Mathf.LerpAngle(_smoothedYaw, _targetYaw, _rotationSmoothing * Time.deltaTime);
        _smoothedPitch = Mathf.LerpAngle(_smoothedPitch, _targetPitch, _rotationSmoothing * Time.deltaTime);

        // XZ et Y avec des smoothings indépendants
        // Le Y plus lent absorbe les snaps verticaux (SnapToGround, chutes, grimpe)
        Vector3 targetPoint = _target.position + _offset;
        _pivotPosition = new Vector3(
            Mathf.Lerp(_pivotPosition.x, targetPoint.x, _followSmoothing * Time.deltaTime),
            Mathf.Lerp(_pivotPosition.y, targetPoint.y, _followSmoothing * Time.deltaTime),
            Mathf.Lerp(_pivotPosition.z, targetPoint.z, _followSmoothing * Time.deltaTime)
        );

        Quaternion rotation = Quaternion.Euler(_smoothedPitch, _smoothedYaw, 0f);
        transform.position = _pivotPosition + rotation * new Vector3(0f, 0f, -_distance);
        transform.LookAt(_pivotPosition);
    }


}
