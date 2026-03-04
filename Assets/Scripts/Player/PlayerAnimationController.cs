using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int FallBlendHash = Animator.StringToHash("FallBlend");
    private static readonly int ClimbHash = Animator.StringToHash("Climb");
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int SweepFallHash = Animator.StringToHash("SweepFall");
    private static readonly int StandUpHash = Animator.StringToHash("StandUp");
    private static readonly int PushHash = Animator.StringToHash("Push");

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    private void OnEnable()
    {
        PlayerEvents.OnSpeedChanged += HandleSpeedChanged;
        PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
        PlayerEvents.OnFallBlendChanged += HandleFallBlendChanged;
        PlayerEvents.OnJumpExecuted += HandleJumpExecuted;
        PlayerEvents.OnClimbStarted += HandleClimbStarted;
        PlayerEvents.OnAimChanged += HandleAimChanged;
        PlayerEvents.OnThrowExecuted += HandleThrowExecuted;
        PlayerEvents.OnSweepFallAnimStarted += HandleSweepFallAnimStarted;
        PlayerEvents.OnStandUpAnimStarted += HandleStandUpAnimStarted;
        PlayerEvents.OnPlayerPushStarted += HandlePlayerPushStarted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnSpeedChanged -= HandleSpeedChanged;
        PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;
        PlayerEvents.OnFallBlendChanged -= HandleFallBlendChanged;
        PlayerEvents.OnJumpExecuted -= HandleJumpExecuted;
        PlayerEvents.OnClimbStarted -= HandleClimbStarted;
        PlayerEvents.OnAimChanged -= HandleAimChanged;
        PlayerEvents.OnThrowExecuted -= HandleThrowExecuted;
        PlayerEvents.OnSweepFallAnimStarted -= HandleSweepFallAnimStarted;
        PlayerEvents.OnStandUpAnimStarted -= HandleStandUpAnimStarted;
        PlayerEvents.OnPlayerPushStarted -= HandlePlayerPushStarted;
    }

    private void HandleSpeedChanged(float speed) => _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    private void HandleGroundedChanged(bool grounded) => _animator.SetBool(IsGroundedHash, grounded);
    private void HandleFallBlendChanged(float blend) => _animator.SetFloat(FallBlendHash, blend, 0.15f, Time.deltaTime);
    private void HandleAimChanged(bool isAiming) => _animator.SetBool(IsAimingHash, isAiming);

    private void HandleJumpExecuted()
    {
        _animator.ResetTrigger(JumpHash);
        _animator.SetTrigger(JumpHash);
    }

    private void HandleClimbStarted()
    {
        _animator.ResetTrigger(ClimbHash);
        _animator.SetTrigger(ClimbHash);
    }

    private void HandleThrowExecuted(Vector3 _)
    {
        _animator.ResetTrigger(ThrowHash);
        _animator.SetTrigger(ThrowHash);
    }

    private void HandleSweepFallAnimStarted()
    {
        _animator.ResetTrigger(SweepFallHash);
        _animator.SetTrigger(SweepFallHash);
    }

    private void HandleStandUpAnimStarted()
    {
        _animator.ResetTrigger(StandUpHash);
        _animator.SetTrigger(StandUpHash);
    }

    /// <summary>Déclenche l'animation de poussée du joueur (sans malette).</summary>
    private void HandlePlayerPushStarted()
    {
        _animator.ResetTrigger(PushHash);
        _animator.SetTrigger(PushHash);
    }
}
