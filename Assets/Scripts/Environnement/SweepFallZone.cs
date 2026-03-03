using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SweepFallZone : MonoBehaviour
{
    [SerializeField] private float _cooldownDuration = 2f;
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private string _playerTag = "Player";

    [Header("Glissade")]
    [SerializeField] private float _slipAcceleration = 2f;   // faible = très glissant
    [SerializeField] private float _normalAcceleration = 10f; // valeur hors zone


    private bool _playerInZone;
    private float _currentSpeed;
    private bool _isSweepActive;
    private bool _isCooldown;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnEnable()
    {
        PlayerEvents.OnSpeedChanged += HandleSpeedChanged;
        PlayerEvents.OnSweepFallCompleted += HandleSweepFallCompleted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnSpeedChanged -= HandleSpeedChanged;
        PlayerEvents.OnSweepFallCompleted -= HandleSweepFallCompleted;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        PlayerEvents.RaiseSlipChanged(_slipAcceleration, true);   // ← isSlipping = true
        _playerInZone = true;
        TryTriggerSweep();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        PlayerEvents.RaiseSlipChanged(_normalAcceleration, false); // ← isSlipping = false
        PlayerEvents.RaiseSlipReset();
        _playerInZone = false;
        _isSweepActive = false;
        _isCooldown = false;
        StopAllCoroutines();
    }

    private void HandleSpeedChanged(float speed) => _currentSpeed = speed;

    private void HandleSweepFallCompleted()
    {
        _isSweepActive = false;

        // Toujours démarrer le cooldown si le joueur est encore dans la zone
        // La vérification du mouvement se fait à la FIN du cooldown
        if (_playerInZone && !_isCooldown)
            StartCoroutine(CooldownThenRetrigger());
    }

    private void TryTriggerSweep()
    {
        if (_isSweepActive || _isCooldown) return;
        _isSweepActive = true;
        PlayerEvents.RaiseSweepFallStarted();
    }

    private IEnumerator CooldownThenRetrigger()
    {
        _isCooldown = true;
        yield return new WaitForSeconds(_cooldownDuration);
        _isCooldown = false;

        // À la fin du cooldown, si le joueur marche → re-trigger
        if (_playerInZone && _currentSpeed > _moveThreshold)
            TryTriggerSweep();
    }
}
