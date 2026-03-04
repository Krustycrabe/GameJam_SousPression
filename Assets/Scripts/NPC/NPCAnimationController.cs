using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int StandUpHash = Animator.StringToHash("StandUp");
    private static readonly int PushHash = Animator.StringToHash("Push");
    private static readonly int IsDancingHash = Animator.StringToHash("IsDancing");
    private static readonly int DanceIndexHash = Animator.StringToHash("DanceIndex");
    private static readonly int IsSweepingHash = Animator.StringToHash("IsSweeping");

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    /// <summary>Vitesse normalisée 0-1 pour le blend tree locomotion.</summary>
    public void SetSpeed(float speed) => _animator.SetFloat(SpeedHash, speed);

    /// <summary>Synchronise le grounded pour les transitions de chute.</summary>
    public void SetGrounded(bool grounded) => _animator.SetBool(IsGroundedHash, grounded);

    /// <summary>Déclenche l'animation de relèvement.</summary>
    public void TriggerGetUp() => _animator.SetTrigger(StandUpHash);

    /// <summary>Déclenche l'animation de poussée.</summary>
    public void TriggerPush() => _animator.SetTrigger(PushHash);

    /// <summary>Active ou désactive l'Animator (ragdoll).</summary>
    public void SetAnimatorEnabled(bool value) => _animator.enabled = value;

    /// <summary>Active/désactive la danse avec un index d'animation configurable.</summary>
    public void SetDancing(bool isDancing, int index = 0)
    {
        _animator.SetBool(IsDancingHash, isDancing);
        _animator.SetInteger(DanceIndexHash, index);
    }

    /// <summary>Active/désactive l'animation de balayage.</summary>
    public void SetSweeping(bool isSweeping) => _animator.SetBool(IsSweepingHash, isSweeping);
}
