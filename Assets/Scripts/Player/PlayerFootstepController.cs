using UnityEngine;

/// <summary>
/// Plays a random footstep sound at a fixed interval while the player is grounded and moving.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerFootstepController : MonoBehaviour
{
    private const float SPEED_THRESHOLD = 0.1f;

    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] [Range(0f, 1f)] private float _volume = 0.8f;
    [SerializeField] private float _footstepInterval = 0.4f;

    private AudioSource _audioSource;
    private float _timer;
    private bool _isGrounded;
    private bool _isMoving;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
        PlayerEvents.OnSpeedChanged += HandleSpeedChanged;
    }

    private void OnDisable()
    {
        PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;
        PlayerEvents.OnSpeedChanged -= HandleSpeedChanged;
    }

    private void Update()
    {
        if (!_isGrounded || !_isMoving) return;

        _timer += Time.deltaTime;
        if (_timer >= _footstepInterval)
        {
            _timer = 0f;
            PlayRandomFootstep();
        }
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        _isGrounded = isGrounded;
        if (!isGrounded) _timer = 0f;
    }

    private void HandleSpeedChanged(float speed)
    {
        _isMoving = speed > SPEED_THRESHOLD;
        if (!_isMoving) _timer = 0f;
    }

    private void PlayRandomFootstep()
    {
        if (_footstepClips == null || _footstepClips.Length == 0) return;

        AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
        _audioSource.PlayOneShot(clip, _volume);
    }
}