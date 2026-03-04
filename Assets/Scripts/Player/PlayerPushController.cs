using System.Collections;
using UnityEngine;

/// <summary>
/// Permet au joueur de pousser un NPC lorsqu'il n'a pas la malette.
/// Utilise le même input que le lancer (left click) — actif uniquement sans malette.
/// </summary>
public class PlayerPushController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform _rightHandBone;

    [Header("Paramètres")]
    [SerializeField] private float _pushForce = 6f;
    [SerializeField] private float _pushRadius = 0.6f;
    [SerializeField] private float _pushHitDelay = 0.2f;  // délai avant détection (sync anim)
    [SerializeField] private float _pushCooldown = 0.8f;

    private bool _hasBriefcase;
    private bool _isPushing;
    private float _lastPushTime = -999f;

    private void OnEnable()
    {
        PlayerEvents.OnBriefcasePickedUp += HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;
        PlayerEvents.OnThrowChargeInput += HandlePushInput;
    }

    private void OnDisable()
    {
        PlayerEvents.OnBriefcasePickedUp -= HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;
        PlayerEvents.OnThrowChargeInput -= HandlePushInput;
    }

    private void HandleBriefcasePickedUp(Transform _) => _hasBriefcase = true;
    private void HandleBriefcaseDropped() => _hasBriefcase = false;

    private void HandlePushInput(bool pressed)
    {
        // Active uniquement si : bouton pressé, pas de malette, pas en cooldown, pas déjà en train de pousser
        if (!pressed || _hasBriefcase || _isPushing) return;
        if (Time.time - _lastPushTime < _pushCooldown) return;

        StartCoroutine(PushCoroutine());
    }

    private IEnumerator PushCoroutine()
    {
        _isPushing = true;
        _lastPushTime = Time.time;

        // Déclenche l'animation de poussée
        PlayerEvents.RaisePlayerPushStarted();

        // Attend que l'animation atteigne le bon frame
        yield return new WaitForSeconds(_pushHitDelay);

        // Détection : OverlapSphere à la position de la main
        Vector3 hitOrigin = _rightHandBone != null ? _rightHandBone.position : transform.position + transform.forward;
        Collider[] hits = Physics.OverlapSphere(hitOrigin, _pushRadius);

        foreach (Collider col in hits)
        {
            NPCController npc = col.GetComponentInParent<NPCController>();
            if (npc == null) continue;

            // Force dans le forward du joueur + légère impulsion vers le haut
            Vector3 force = (transform.forward + Vector3.up * 0.3f).normalized * _pushForce;
            npc.OnHit(force);

            PlayerEvents.RaisePlayerPushHit();
            break; // un seul NPC touché par poussée
        }

        _isPushing = false;
    }
}
