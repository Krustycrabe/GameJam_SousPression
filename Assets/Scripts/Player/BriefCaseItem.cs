using UnityEngine;

public class BriefcaseItem : MonoBehaviour
{
    [Header("Physique au sol")]
    [SerializeField] private float _airDrag = 0.1f;
    [SerializeField] private float _groundDrag = 4f;
    [SerializeField] private float _angularDrag = 3f;

    [Header("Impact NPC")]
    [SerializeField] private float _npcHitForce = 8f;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Transform _meshTransform; // malette — l'enfant avec le Rigidbody

    // Pose de BriefCaseItem relatif à RightHand
    private Vector3 _rootInitialLocalPosition;
    private Quaternion _rootInitialLocalRotation;

    // Pose de malette relatif à BriefCaseItem (restaurée après physique)
    private Vector3 _meshInitialLocalPosition;
    private Quaternion _meshInitialLocalRotation;

    public bool IsHeld { get; private set; }

    private void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _collider = GetComponentInChildren<Collider>();
        _meshTransform = _rigidbody.transform; // malette

        // BriefCaseItem est lui-même le root géré — PAS son parent (le bone)
        _rootInitialLocalPosition = transform.localPosition;
        _rootInitialLocalRotation = transform.localRotation;

        // Pose initiale de malette relatif à BriefCaseItem
        _meshInitialLocalPosition = _meshTransform.localPosition;
        _meshInitialLocalRotation = _meshTransform.localRotation;

        _rigidbody.angularDamping = _angularDrag;
        SetAirPhysics();

        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        IsHeld = true;
    }

    /// <summary>Attache la malette à la main et restaure la pose complète.</summary>
    public void AttachToHand(Transform handBone)
    {
        IsHeld = true;
        _rigidbody.isKinematic = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _collider.enabled = false;

        // Reparente BriefCaseItem sous la main
        transform.SetParent(handBone, worldPositionStays: false);
        transform.SetLocalPositionAndRotation(_rootInitialLocalPosition, _rootInitialLocalRotation);

        // Restaure la pose de malette (dérive physique accumulée)
        _meshTransform.SetLocalPositionAndRotation(_meshInitialLocalPosition, _meshInitialLocalRotation);
    }

    /// <summary>Détache BriefCaseItem dans le monde et active la physique.</summary>
    public void Detach()
    {
        IsHeld = false;
        transform.SetParent(null); // détache BriefCaseItem, pas le bone
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        SetAirPhysics();
    }

    /// <summary>Détache et applique une vélocité initiale.</summary>
    public void Launch(Vector3 velocity)
    {
        Detach();
        _rigidbody.linearVelocity = velocity;
    }

    /// <summary>Appelé par BriefcaseCollisionProxy depuis malette.</summary>
    public void HandleCollisionEnter(Collision collision)
    {
        _rigidbody.linearDamping = _groundDrag;

        if (!IsHeld)
        {
            NPCController npc = collision.gameObject.GetComponentInParent<NPCController>();
            if (npc != null)
            {
                Vector3 force = _rigidbody.linearVelocity.normalized * _npcHitForce;
                npc.OnHit(force);
            }
        }
    }

    /// <summary>Appelé par BriefcaseCollisionProxy depuis malette.</summary>
    public void HandleCollisionExit(Collision collision) => SetAirPhysics();

    private void SetAirPhysics() => _rigidbody.linearDamping = _airDrag;
}
