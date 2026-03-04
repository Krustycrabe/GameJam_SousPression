using UnityEngine;

/// <summary>
/// Proxy sur le GameObject portant le Rigidbody.
/// Relaie les événements de collision à BriefcaseItem (parent).
/// </summary>
public class BriefcaseCollisionProxy : MonoBehaviour
{
    private BriefcaseItem _owner;

    private void Awake() => _owner = GetComponentInParent<BriefcaseItem>();

    private void OnCollisionEnter(Collision collision) => _owner?.HandleCollisionEnter(collision);
    private void OnCollisionExit(Collision collision) => _owner?.HandleCollisionExit(collision);
}
