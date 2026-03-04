using UnityEngine;

/// <summary>
/// Trigger de détection du player pour l'ascenseur.
/// À placer sur un enfant de l'ELEVATOR avec un BoxCollider (isTrigger activé auto).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ElevatorTriggerZone : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField] private ElevatorController _elevator;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag))
            _elevator.OnPlayerEntered(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PlayerTag))
            _elevator.OnPlayerExited();
    }
}
