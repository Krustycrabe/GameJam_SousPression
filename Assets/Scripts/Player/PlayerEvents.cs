using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<Vector2> OnMoveInput;
    public static event Action<Vector2> OnLookInput;
    public static event Action OnJumpInput;

    public static event Action<Transform> OnPlayerTransformReady;
    public static event Action<float> OnSpeedChanged;
    public static event Action<bool> OnGroundedChanged;
    public static event Action<float> OnFallBlendChanged;

    public static event Action<float> OnJumpForceRequested;
    public static event Action OnJumpExecuted;

    public static event Action OnClimbStarted;    // ← nouveau
    public static event Action OnClimbCompleted;  // ← nouveau

    public static void RaiseMoveInput(Vector2 direction) => OnMoveInput?.Invoke(direction);
    public static void RaiseLookInput(Vector2 delta) => OnLookInput?.Invoke(delta);
    public static void RaiseJumpInput() => OnJumpInput?.Invoke();
    public static void RaisePlayerTransformReady(Transform t) => OnPlayerTransformReady?.Invoke(t);
    public static void RaiseSpeedChanged(float speed) => OnSpeedChanged?.Invoke(speed);
    public static void RaiseGroundedChanged(bool isGrounded) => OnGroundedChanged?.Invoke(isGrounded);
    public static void RaiseFallBlendChanged(float blend) => OnFallBlendChanged?.Invoke(blend);
    public static void RaiseJumpForceRequested(float force) => OnJumpForceRequested?.Invoke(force);
    public static void RaiseJumpExecuted() => OnJumpExecuted?.Invoke();
    public static void RaiseClimbStarted() => OnClimbStarted?.Invoke();   // ← nouveau
    public static void RaiseClimbCompleted() => OnClimbCompleted?.Invoke(); // ← nouveau

    public static event Action OnClimbAnimationEnd;
    public static void RaiseClimbAnimationEnd() => OnClimbAnimationEnd?.Invoke();

}
