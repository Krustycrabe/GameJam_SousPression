using System.Collections;
using UnityEngine;

public class PlayerSweepFallController : MonoBehaviour
{
    [Header("Durées — caler avec les clips FBX")]
    [SerializeField] private float _sweepFallDuration = 1.5f;
    [SerializeField] private float _standUpDuration = 1.2f;

    private Animator _animator;
    private bool _isActive;

    private void Awake() => _animator = GetComponent<Animator>();

    private void OnEnable() => PlayerEvents.OnSweepFallStarted += HandleSweepFallStarted;
    private void OnDisable() => PlayerEvents.OnSweepFallStarted -= HandleSweepFallStarted;

    private void HandleSweepFallStarted()
    {
        if (_isActive) return;
        StartCoroutine(SweepFallSequence());
    }

    private IEnumerator SweepFallSequence()
    {
        _isActive = true;

        // Désactive le root motion : le personnage reste au sol,
        // la physique (gravité) gère la position Y
        _animator.applyRootMotion = false;

        PlayerEvents.RaiseSweepFallAnimStarted();
        yield return new WaitForSeconds(_sweepFallDuration);

        PlayerEvents.RaiseStandUpAnimStarted();
        yield return new WaitForSeconds(_standUpDuration);

        _isActive = false;
        PlayerEvents.RaiseSweepFallCompleted();
    }
}
