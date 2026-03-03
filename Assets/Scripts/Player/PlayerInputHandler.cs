using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Lit les inputs depuis l'InputActionAsset et les publie via PlayerEvents.</summary>
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;

    private void Awake()
    {
        InputActionMap playerMap = _inputActions.FindActionMap("IMC_Player", throwIfNotFound: true);

        _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        _lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        _jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();

        _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();

        _jumpAction.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        PlayerEvents.RaiseMoveInput(_moveAction.ReadValue<Vector2>());
        PlayerEvents.RaiseLookInput(_lookAction.ReadValue<Vector2>());
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx) => PlayerEvents.RaiseJumpInput();
}
