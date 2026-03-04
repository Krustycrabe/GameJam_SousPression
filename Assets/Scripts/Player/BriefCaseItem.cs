using UnityEngine;

public class BriefcaseItem : MonoBehaviour
{
    [Header("Physique au sol")]
    [SerializeField] private float _airDrag = 0.1f;
    [SerializeField] private float _groundDrag = 4f;
    [SerializeField] private float _angularDrag = 3f;

    [Header("Impact NPC")]
    [SerializeField] private float _npcHitForce = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _landClip;
    [SerializeField] private AudioClip _npcHitClip;
    [SerializeField] private AudioClip _pickupClip;
    [SerializeField] [Range(0f, 1f)] private float _audioVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float _landVolume = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float _pickupVolume = 1f;
    [SerializeField] private float _landVelocityThreshold = 2f;

    [Header("Audio - Spatialisation 3D")]
    [SerializeField] [Range(0f, 1f)] private float _spatialBlend = 1f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 20f;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Transform _meshTransform;

    private Vector3 _rootInitialLocalPosition;
    private Quaternion _rootInitialLocalRotation;
    private Vector3 _rootInitialLocalScale;

    private Vector3 _meshInitialLocalPosition;
    private Quaternion _meshInitialLocalRotation;

    private bool _hasLanded;



    public bool IsHeld { get; private set; }

    private void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _collider = GetComponentInChildren<Collider>();
        _meshTransform = _rigidbody.transform;

        _rootInitialLocalPosition = transform.localPosition;
        _rootInitialLocalRotation = transform.localRotation;

        _meshInitialLocalPosition = _meshTransform.localPosition;
        _meshInitialLocalRotation = _meshTransform.localRotation;

        _rigidbody.angularDamping = _angularDrag;
        SetAirPhysics();

        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        IsHeld = true;

        _rootInitialLocalPosition = transform.localPosition;
        _rootInitialLocalRotation = transform.localRotation;
        _rootInitialLocalScale = transform.localScale;

        ConfigureSpatialAudio();
    }

    /// <summary>Attache la malette à la main et restaure la pose complète.</summary>
    public void AttachToHand(Transform handBone)
    {
        IsHeld = true;
        _rigidbody.isKinematic = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _collider.enabled = false;

        transform.SetParent(handBone, worldPositionStays: false);
        transform.localScale = _rootInitialLocalScale;                              // ← ajoute cette ligne
        transform.SetLocalPositionAndRotation(_rootInitialLocalPosition, _rootInitialLocalRotation);


        PlaySound(_pickupClip, _pickupVolume);
    }

    /// <summary>Détache BriefCaseItem dans le monde et active la physique.</summary>
    public void Detach()
    {
        IsHeld = false;
        _hasLanded = false;
        transform.SetParent(null);
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

        if (IsHeld) return;

        NPCController npc = collision.gameObject.GetComponentInParent<NPCController>();
        if (npc != null)
        {
            Vector3 force = _rigidbody.linearVelocity.normalized * _npcHitForce;
            npc.OnHit(force);
            PlaySound(_npcHitClip, _audioVolume);
        }
        else if (!_hasLanded && collision.relativeVelocity.magnitude >= _landVelocityThreshold)
        {
            _hasLanded = true;
            PlaySound(_landClip, _landVolume);
        }
    }

    /// <summary>Appelé par BriefcaseCollisionProxy depuis malette.</summary>
    public void HandleCollisionExit(Collision collision) => SetAirPhysics();

    private void SetAirPhysics() => _rigidbody.linearDamping = _airDrag;

    private void ConfigureSpatialAudio()
    {
        if (_audioSource == null) return;
        _audioSource.spatialBlend = _spatialBlend;
        _audioSource.minDistance = _minDistance;
        _audioSource.maxDistance = _maxDistance;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (_audioSource == null || clip == null) return;
        _audioSource.PlayOneShot(clip, volume);
    }
}
