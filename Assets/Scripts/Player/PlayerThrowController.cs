using UnityEngine;

/// <summary>
/// Gère le chargement et le lancer de la malette.
/// Actif uniquement en mode visée. Annulé si la visée est relâchée avant le lancer.
/// </summary>
public class PlayerThrowController : MonoBehaviour
{
    [Header("Paramètres du lancer")]
    [SerializeField] private float _minThrowSpeed = 5f;
    [SerializeField] private float _maxThrowSpeed = 18f;
    [SerializeField] private float _arcLift = 0.4f;   // composante Y ajoutée à la direction
    [SerializeField] private float _chargeSpeed = 1f;     // unités de charge par seconde
    [SerializeField] private float _minChargeToThrow = 0.05f;

    private bool _isAiming;
    private bool _isCharging;
    private float _currentCharge;

    private void OnEnable()
    {
        PlayerEvents.OnAimChanged += HandleAimChanged;
        PlayerEvents.OnThrowChargeInput += HandleThrowChargeInput;
    }

    private void OnDisable()
    {
        PlayerEvents.OnAimChanged -= HandleAimChanged;
        PlayerEvents.OnThrowChargeInput -= HandleThrowChargeInput;
    }

    private void HandleAimChanged(bool isAiming)
    {
        _isAiming = isAiming;
        if (!isAiming) CancelCharge();
    }

    private void HandleThrowChargeInput(bool isPressed)
    {
        if (isPressed && _isAiming)
        {
            _isCharging = true;
        }
        else if (!isPressed && _isCharging)
        {
            if (_currentCharge >= _minChargeToThrow) ExecuteThrow();
            else CancelCharge();
        }
    }

    private void Update()
    {
        if (!_isCharging) return;

        _currentCharge = Mathf.Min(1f, _currentCharge + _chargeSpeed * Time.deltaTime);
        PlayerEvents.RaiseThrowChargeChanged(_currentCharge);
    }

    /// <summary>Calcule la vélocité de lancer et fire l'event.</summary>
    private void ExecuteThrow()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 throwDir = (camForward + Vector3.up * _arcLift).normalized;
        float speed = Mathf.Lerp(_minThrowSpeed, _maxThrowSpeed, _currentCharge);
        Vector3 velocity = throwDir * speed;

        PlayerEvents.RaiseThrowExecuted(velocity);
        ResetCharge();
    }

    private void CancelCharge() => ResetCharge();

    private void ResetCharge()
    {
        _isCharging = false;
        _currentCharge = 0f;
        PlayerEvents.RaiseThrowChargeChanged(0f);
    }
}
