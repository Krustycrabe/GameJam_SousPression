using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundZone : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
    [SerializeField] private float _fadeDistance = 3f;

    private AudioSource _audioSource;
    private Transform _playerTransform;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _clip;
        _audioSource.spatialBlend = 0f;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0f;
        _audioSource.Play();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        Vector3 local = transform.InverseTransformPoint(_playerTransform.position);
        Vector3 d = new Vector3(
            Mathf.Max(Mathf.Abs(local.x) - 0.5f, 0f),
            Mathf.Max(Mathf.Abs(local.y) - 0.5f, 0f),
            Mathf.Max(Mathf.Abs(local.z) - 0.5f, 0f)
        );

        _audioSource.volume = Mathf.InverseLerp(_fadeDistance, 0f, d.magnitude) * _volume;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        float fx = _fadeDistance / transform.lossyScale.x;
        float fy = _fadeDistance / transform.lossyScale.y;
        float fz = _fadeDistance / transform.lossyScale.z;
        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one + new Vector3(fx, fy, fz) * 2f);
    }
}