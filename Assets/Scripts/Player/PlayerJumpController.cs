using UnityEngine;

/// <summary>Valide les conditions de saut et envoie la force via PlayerEvents.</summary>
public class PlayerJumpController : MonoBehaviour
{
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravityScale = 2f;
    [SerializeField] private float _landingCooldown = 0.15f;

    private const float Gravity = -9.81f;

    private bool _isGrounded = true;
    private float _cooldownTimer = 0f;

    private void OnEnable()
    {
        PlayerEvents.OnJumpInput += HandleJumpInput;
        PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
    }

    private void OnDisable()
    {
        PlayerEvents.OnJumpInput -= HandleJumpInput;
        PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        _isGrounded = isGrounded;

        // Démarre le cooldown à l'atterrissage pour éviter la race condition
        if (isGrounded)
            _cooldownTimer = _landingCooldown;
    }

    private void HandleJumpInput()
    {
        if (!_isGrounded || _cooldownTimer > 0f) return;

        float jumpForce = Mathf.Sqrt(-2f * Gravity * _gravityScale * _jumpHeight);
        PlayerEvents.RaiseJumpForceRequested(jumpForce);
        PlayerEvents.RaiseJumpExecuted();
    }
}
