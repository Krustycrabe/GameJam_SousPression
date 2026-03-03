using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClimbDetector : MonoBehaviour
{
    [SerializeField] private string _climbableTag = "Climbable";

    public Collider DetectedClimbable { get; private set; }

    // isTrigger configuré directement dans l'Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_climbableTag))
            DetectedClimbable = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == DetectedClimbable)
            DetectedClimbable = null;
    }
}
