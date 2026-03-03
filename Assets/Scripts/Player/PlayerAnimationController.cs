using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int FallBlendHash = Animator.StringToHash("FallBlend");
    private static readonly int ClimbHash = Animator.StringToHash("Climb");

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    private void OnEnable()
    {
        PlayerEvents.OnSpeedChanged += HandleSpeedChanged;
        PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
        PlayerEvents.OnFallBlendChanged += HandleFallBlendChanged;
        PlayerEvents.OnJumpExecuted += HandleJumpExecuted;
        PlayerEvents.OnClimbStarted += HandleClimbStarted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnSpeedChanged -= HandleSpeedChanged;
        PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;
        PlayerEvents.OnFallBlendChanged -= HandleFallBlendChanged;
        PlayerEvents.OnJumpExecuted -= HandleJumpExecuted;
        PlayerEvents.OnClimbStarted -= HandleClimbStarted;
    }

    private void HandleSpeedChanged(float speed) => _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    private void HandleGroundedChanged(bool isGrounded) => _animator.SetBool(IsGroundedHash, isGrounded);
    private void HandleFallBlendChanged(float blend) => _animator.SetFloat(FallBlendHash, blend, 0.15f, Time.deltaTime);

    private void HandleJumpExecuted()
    {
        _animator.ResetTrigger(JumpHash);
        _animator.SetTrigger(JumpHash);
    }

    private void HandleClimbStarted()
    {
        _animator.ResetTrigger(ClimbHash);
        _animator.SetTrigger(ClimbHash);
        // applyRootMotion géré par PlayerClimbController
    }

}
