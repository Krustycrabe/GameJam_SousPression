using UnityEngine;

/// <summary>
/// Zone de son sphérique avec fade out configurable.
/// Plein volume dans _zoneRadius, silence à _zoneRadius + _fadeDistance.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class SoundZone : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private float _zoneRadius = 10f;
    [SerializeField] private float _fadeDistance = 3f;

    [Header("Audio")]
    [SerializeField] [Range(0f, 1f)] private float _masterVolume = 1f;

    private AudioSource _audioSource;
    private SphereCollider _sphereCollider;
    private Transform _playerTransform;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.loop = true;
        _audioSource.volume = 0f;
        _audioSource.Play();

        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.isTrigger = true;
        _sphereCollider.radius = _zoneRadius + _fadeDistance;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        float targetVolume = 0f;

        if (distance <= _zoneRadius)
            targetVolume = 1f;
        else if (distance <= _zoneRadius + _fadeDistance)
            targetVolume = Mathf.InverseLerp(_zoneRadius + _fadeDistance, _zoneRadius, distance);

        _audioSource.volume = targetVolume * _masterVolume;
    }

    private void OnDrawGizmosSelected()
    {
        // Zone plein volume (vert)
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, _zoneRadius);

        // Zone de fade (jaune)
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, _zoneRadius + _fadeDistance);
    }
}
