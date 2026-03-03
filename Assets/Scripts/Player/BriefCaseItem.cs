using UnityEngine;

public class BriefcaseItem : MonoBehaviour
{
    [Header("Physique au sol")]
    [SerializeField] private float _airDrag = 0.1f;
    [SerializeField] private float _groundDrag = 4f;
    [SerializeField] private float _angularDrag = 3f;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Transform _managedRoot;

    // Pose initiale de BriefCaseItem (le parent positionné)
    private Vector3 _rootInitialLocalPosition;
    private Quaternion _rootInitialLocalRotation;

    // Pose initiale de BriefCase (le mesh — doit revenir à (0,0,0) relatif au parent)
    private Vector3 _meshInitialLocalPosition;
    private Quaternion _meshInitialLocalRotation;

    public bool IsHeld { get; private set; }

    private void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _collider = GetComponentInChildren<Collider>();

        _managedRoot = transform.parent;

        // Sauvegarde les deux poses — avant toute physique
        _rootInitialLocalPosition = _managedRoot.localPosition;
        _rootInitialLocalRotation = _managedRoot.localRotation;
        _meshInitialLocalPosition = transform.localPosition;
        _meshInitialLocalRotation = transform.localRotation;

        _rigidbody.angularDamping = _angularDrag;
        SetAirPhysics();

        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        IsHeld = true;
    }

    /// <summary>Attache en restaurant la pose complète : parent + mesh child.</summary>
    public void AttachToHand(Transform handBone)
    {
        IsHeld = true;
        _rigidbody.isKinematic = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _collider.enabled = false;

        _managedRoot.SetParent(handBone, worldPositionStays: false);
        _managedRoot.SetLocalPositionAndRotation(_rootInitialLocalPosition, _rootInitialLocalRotation);

        // Réinitialise la dérive physique accumulée sur le mesh child
        transform.SetLocalPositionAndRotation(_meshInitialLocalPosition, _meshInitialLocalRotation);
    }

    /// <summary>Détache BriefCaseItem dans le monde et active la physique.</summary>
    public void Detach()
    {
        IsHeld = false;
        _managedRoot.SetParent(null);
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

    private void OnCollisionEnter(Collision _) => _rigidbody.linearDamping = _groundDrag;
    private void OnCollisionExit(Collision _) => SetAirPhysics();
    private void SetAirPhysics() => _rigidbody.linearDamping = _airDrag;
}
