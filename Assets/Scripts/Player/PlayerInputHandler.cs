using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _aimAction;
    private InputAction _throwChargeAction;

    private void Awake()
    {
        InputActionMap playerMap = _inputActions.FindActionMap("IMC_Player", throwIfNotFound: true);

        _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        _lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        _jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
        _interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
        _aimAction = playerMap.FindAction("Aim", throwIfNotFound: true);
        _throwChargeAction = playerMap.FindAction("ThrowCharge", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _interactAction.Enable();
        _aimAction.Enable();
        _throwChargeAction.Enable();

        _jumpAction.performed += OnJumpPerformed;
        _interactAction.performed += OnInteractPerformed;
        _aimAction.started += OnAimStarted;
        _aimAction.canceled += OnAimCanceled;
        _throwChargeAction.started += OnThrowChargeStarted;
        _throwChargeAction.canceled += OnThrowChargeCanceled;
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _interactAction.Disable();
        _aimAction.Disable();
        _throwChargeAction.Disable();

        _jumpAction.performed -= OnJumpPerformed;
        _interactAction.performed -= OnInteractPerformed;
        _aimAction.started -= OnAimStarted;
        _aimAction.canceled -= OnAimCanceled;
        _throwChargeAction.started -= OnThrowChargeStarted;
        _throwChargeAction.canceled -= OnThrowChargeCanceled;
    }

    private void Update()
    {
        PlayerEvents.RaiseMoveInput(_moveAction.ReadValue<Vector2>());
        PlayerEvents.RaiseLookInput(_lookAction.ReadValue<Vector2>());
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx) => PlayerEvents.RaiseJumpInput();
    private void OnInteractPerformed(InputAction.CallbackContext ctx) => PlayerEvents.RaisePickupInput();
    private void OnAimStarted(InputAction.CallbackContext ctx) => PlayerEvents.RaiseAimInput(true);
    private void OnAimCanceled(InputAction.CallbackContext ctx) => PlayerEvents.RaiseAimInput(false);
    private void OnThrowChargeStarted(InputAction.CallbackContext ctx) => PlayerEvents.RaiseThrowChargeInput(true);
    private void OnThrowChargeCanceled(InputAction.CallbackContext ctx) => PlayerEvents.RaiseThrowChargeInput(false);
}
