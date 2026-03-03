using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrampolineZone : MonoBehaviour
{
    [SerializeField] private float _verticalForce = 12f;
    [SerializeField] private float _horizontalForce = 6f;
    [SerializeField] private string _playerTag = "Player";

    private bool _playerInZone;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnEnable() => PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
    private void OnDisable() => PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _playerInZone = true;
        Launch();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _playerInZone = false;
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        if (isGrounded && _playerInZone) Launch();
    }

    private void Launch()
    {
        // La direction horizontale est calculée dans PlayerMovementController via _moveInput
        PlayerEvents.RaiseTrampolineBounce(_verticalForce, _horizontalForce);
    }
}
