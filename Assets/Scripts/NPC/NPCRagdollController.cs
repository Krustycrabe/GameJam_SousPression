using UnityEngine;

public class NPCRagdollController : MonoBehaviour
{
    [Tooltip("Assign le bone Hips/Pelvis — racine du ragdoll.")]
    [SerializeField] private Transform _hipBone;
    [SerializeField] private LayerMask _groundMask = ~0;

    private Rigidbody[] _ragdollBodies;
    private Collider[] _ragdollColliders;
    private CharacterController _cc;
    private CapsuleCollider _bodyCollider;
    private NPCAnimationController _animController;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _bodyCollider = GetComponent<CapsuleCollider>();
        _animController = GetComponent<NPCAnimationController>();

        // Exclut le Rigidbody root — évite les conflits avec TrampolineBounceSequence
        _ragdollBodies = System.Array.FindAll(
            GetComponentsInChildren<Rigidbody>(),
            rb => rb.gameObject != gameObject
        );

        _ragdollColliders = GetComponentsInChildren<Collider>(true);

        SetRagdollActive(false);
    }

    /// <summary>Active le ragdoll et applique une impulsion sur le hip.</summary>
    public void EnableRagdoll(Vector3 impulse)
    {
        if (_cc) _cc.enabled = false;
        if (_bodyCollider) _bodyCollider.enabled = false;

        _animController.SetAnimatorEnabled(false);
        SetRagdollActive(true);

        if (_hipBone != null && _hipBone.TryGetComponent<Rigidbody>(out var hipRb))
            hipRb.AddForce(impulse, ForceMode.Impulse);
    }

    /// <summary>Désactive le ragdoll et replace le root au sol sous le hip.</summary>
    public void DisableRagdoll()
    {
        SnapRootToGround();
        SetRagdollActive(false);

        if (_cc) _cc.enabled = true;
        if (_bodyCollider) _bodyCollider.enabled = true;

        _animController.SetAnimatorEnabled(true);
    }

    private void SnapRootToGround()
    {
        if (_hipBone == null) return;

        Vector3 origin = _hipBone.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, _groundMask, QueryTriggerInteraction.Ignore))
            transform.position = hit.point;
        else
            transform.position = new Vector3(_hipBone.position.x, transform.position.y, _hipBone.position.z);
    }

    private void SetRagdollActive(bool active)
    {
        foreach (var rb in _ragdollBodies)
            rb.isKinematic = !active;

        foreach (var col in _ragdollColliders)
        {
            if (col.gameObject == gameObject) continue; // skip root
            col.enabled = active;
        }
    }
}
