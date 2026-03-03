using UnityEngine;

/// <summary>
/// Gère l'entrée et la sortie du mode visée.
/// Nécessite de tenir la malette. Fire OnAimChanged pour notifier les autres systèmes.
/// </summary>
public class PlayerAimController : MonoBehaviour
{
    private bool _isHoldingBriefcase;
    private bool _isAiming;

    private void OnEnable()
    {
        PlayerEvents.OnAimInput += HandleAimInput;
        PlayerEvents.OnBriefcasePickedUp += HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;
        PlayerEvents.OnThrowExecuted += HandleThrowExecuted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnAimInput -= HandleAimInput;
        PlayerEvents.OnBriefcasePickedUp -= HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;
        PlayerEvents.OnThrowExecuted -= HandleThrowExecuted;
    }

    private void HandleBriefcasePickedUp(Transform _) => _isHoldingBriefcase = true;

    private void HandleBriefcaseDropped()
    {
        _isHoldingBriefcase = false;
        ExitAim();
    }

    private void HandleThrowExecuted(Vector3 _) => ExitAim();

    private void HandleAimInput(bool isPressed)
    {
        if (isPressed && _isHoldingBriefcase) EnterAim();
        else ExitAim();
    }

    private void EnterAim()
    {
        if (_isAiming) return;
        _isAiming = true;
        PlayerEvents.RaiseAimChanged(true);
    }

    private void ExitAim()
    {
        if (!_isAiming) return;
        _isAiming = false;
        PlayerEvents.RaiseAimChanged(false);
    }
}
