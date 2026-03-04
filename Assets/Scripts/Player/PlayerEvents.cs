using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<Vector2> OnMoveInput;
    public static event Action<Vector2> OnLookInput;
    public static event Action OnJumpInput;
    public static event Action OnPickupInput;
    public static event Action<bool> OnAimInput;
    public static event Action<bool> OnThrowChargeInput;

    public static event Action<Transform> OnPlayerTransformReady;
    public static event Action<float> OnSpeedChanged;
    public static event Action<bool> OnGroundedChanged;
    public static event Action<float> OnFallBlendChanged;

    public static event Action<float> OnJumpForceRequested;
    public static event Action OnJumpExecuted;

    public static event Action OnClimbStarted;
    public static event Action OnClimbCompleted;
    public static event Action OnClimbAnimationEnd;

    public static event Action<Transform> OnBriefcasePickedUp;
    public static event Action OnBriefcaseDropped;
    public static event Action<bool> OnAimChanged;
    public static event Action<float> OnThrowChargeChanged;
    public static event Action<Vector3> OnThrowExecuted;

    public static event Action OnPlayerPushStarted;
    public static event Action OnPlayerPushHit;

    public static void RaiseMoveInput(Vector2 d) => OnMoveInput?.Invoke(d);
    public static void RaiseLookInput(Vector2 d) => OnLookInput?.Invoke(d);
    public static void RaiseJumpInput() => OnJumpInput?.Invoke();
    public static void RaisePickupInput() => OnPickupInput?.Invoke();
    public static void RaiseAimInput(bool v) => OnAimInput?.Invoke(v);
    public static void RaiseThrowChargeInput(bool v) => OnThrowChargeInput?.Invoke(v);
    public static void RaisePlayerTransformReady(Transform t) => OnPlayerTransformReady?.Invoke(t);
    public static void RaiseSpeedChanged(float v) => OnSpeedChanged?.Invoke(v);
    public static void RaiseGroundedChanged(bool v) => OnGroundedChanged?.Invoke(v);
    public static void RaiseFallBlendChanged(float v) => OnFallBlendChanged?.Invoke(v);
    public static void RaiseJumpForceRequested(float v) => OnJumpForceRequested?.Invoke(v);
    public static void RaiseJumpExecuted() => OnJumpExecuted?.Invoke();
    public static void RaiseClimbStarted() => OnClimbStarted?.Invoke();
    public static void RaiseClimbCompleted() => OnClimbCompleted?.Invoke();
    public static void RaiseClimbAnimationEnd() => OnClimbAnimationEnd?.Invoke();
    public static void RaiseBriefcasePickedUp(Transform t) => OnBriefcasePickedUp?.Invoke(t);
    public static void RaiseBriefcaseDropped() => OnBriefcaseDropped?.Invoke();
    public static void RaiseAimChanged(bool v) => OnAimChanged?.Invoke(v);
    public static void RaiseThrowChargeChanged(float v) => OnThrowChargeChanged?.Invoke(v);
    public static void RaiseThrowExecuted(Vector3 v) => OnThrowExecuted?.Invoke(v);

    public static void RaisePlayerPushStarted() => OnPlayerPushStarted?.Invoke();
    public static void RaisePlayerPushHit() => OnPlayerPushHit?.Invoke();

    public static event Action OnSweepFallStarted;
    public static event Action OnSweepFallCompleted;

    public static event Action<float, float> OnTrampolineBounce;
    public static event Action<float, bool> OnSlipChanged;

    public static event Action OnSlipReset;
    public static void RaiseSlipReset() => OnSlipReset?.Invoke();

    public static void RaiseSlipChanged(float acceleration, bool isSlipping)
    => OnSlipChanged?.Invoke(acceleration, isSlipping);


    public static void RaiseSweepFallStarted() => OnSweepFallStarted?.Invoke();
    public static void RaiseSweepFallCompleted() => OnSweepFallCompleted?.Invoke();
    public static void RaiseTrampolineBounce(float vertical, float horizontal) => OnTrampolineBounce?.Invoke(vertical, horizontal);
    public static event Action OnSweepFallAnimStarted;
    public static event Action OnStandUpAnimStarted;

    public static void RaiseSweepFallAnimStarted() => OnSweepFallAnimStarted?.Invoke();
    public static void RaiseStandUpAnimStarted() => OnStandUpAnimStarted?.Invoke();

}
