using UnityEngine;

/// <summary>
/// Affiche l'arc de lancer en temps réel via LineRenderer.
/// Met à jour la trajectoire à chaque changement de charge.
/// Pose un marqueur de landing au point d'impact détecté.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ThrowTrajectoryRenderer : MonoBehaviour
{
    [Header("Paramètres — doivent correspondre à PlayerThrowController")]
    [SerializeField] private float _minThrowSpeed = 5f;
    [SerializeField] private float _maxThrowSpeed = 18f;
    [SerializeField] private float _arcLift = 0.4f;

    [Header("Trajectoire")]
    [SerializeField] private int _sampleCount = 30;
    [SerializeField] private float _timeStep = 0.1f;
    [SerializeField] private Color _colorMin = Color.green;
    [SerializeField] private Color _colorMax = Color.red;

    [Header("Marqueur de landing")]
    [SerializeField] private GameObject _landingMarker;

    private LineRenderer _lineRenderer;
    private Transform _briefcaseTransform;
    private float _currentCharge;
    private bool _isVisible;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = 0.04f;
        _lineRenderer.endWidth = 0.01f;

        SetVisible(false);
    }

    private void OnEnable()
    {
        PlayerEvents.OnAimChanged += HandleAimChanged;
        PlayerEvents.OnThrowChargeChanged += HandleThrowChargeChanged;
        PlayerEvents.OnBriefcasePickedUp += HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;
        PlayerEvents.OnThrowExecuted += HandleThrowExecuted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnAimChanged -= HandleAimChanged;
        PlayerEvents.OnThrowChargeChanged -= HandleThrowChargeChanged;
        PlayerEvents.OnBriefcasePickedUp -= HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;
        PlayerEvents.OnThrowExecuted -= HandleThrowExecuted;
    }

    private void HandleBriefcasePickedUp(Transform t) => _briefcaseTransform = t;
    private void HandleBriefcaseDropped() => _briefcaseTransform = null;
    private void HandleThrowExecuted(Vector3 _) => SetVisible(false);

    private void HandleAimChanged(bool isAiming) => SetVisible(isAiming);

    private void HandleThrowChargeChanged(float charge)
    {
        _currentCharge = charge;
        if (_isVisible) RefreshTrajectory();
    }

    private void LateUpdate()
    {
        // Refresh chaque frame en mode visée pour suivre les mouvements de caméra
        if (_isVisible) RefreshTrajectory();
    }

    private void RefreshTrajectory()
    {
        if (_briefcaseTransform == null) return;

        Vector3 startPos = _briefcaseTransform.position;
        Vector3 velocity = ComputeThrowVelocity(_currentCharge);

        // Couleur selon la charge
        Color color = Color.Lerp(_colorMin, _colorMax, _currentCharge);
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;

        _lineRenderer.positionCount = _sampleCount;

        Vector3 landingPoint = Vector3.zero;
        bool foundLanding = false;
        int usedSamples = _sampleCount;

        for (int i = 0; i < _sampleCount; i++)
        {
            float t = i * _timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * Physics.gravity * t * t;

            // Détection d'impact avec la géométrie
            if (i > 0)
            {
                Vector3 prev = _lineRenderer.GetPosition(i - 1);
                if (Physics.Linecast(prev, point, out RaycastHit hit))
                {
                    _lineRenderer.SetPosition(i, hit.point);
                    usedSamples = i + 1;
                    landingPoint = hit.point;
                    foundLanding = true;
                    break;
                }
            }

            _lineRenderer.SetPosition(i, point);
        }

        _lineRenderer.positionCount = usedSamples;

        // Marqueur de landing
        if (_landingMarker != null)
        {
            _landingMarker.SetActive(foundLanding);
            if (foundLanding)
                _landingMarker.transform.position = landingPoint + Vector3.up * 0.01f;
        }
    }

    private Vector3 ComputeThrowVelocity(float charge)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 throwDir = (camForward + Vector3.up * _arcLift).normalized;
        float speed = Mathf.Lerp(_minThrowSpeed, _maxThrowSpeed, charge);
        return throwDir * speed;
    }

    private void SetVisible(bool visible)
    {
        _isVisible = visible;
        _lineRenderer.enabled = visible;
        if (_landingMarker != null) _landingMarker.SetActive(visible && _isVisible);
        if (!visible) _lineRenderer.positionCount = 0;
    }
}
