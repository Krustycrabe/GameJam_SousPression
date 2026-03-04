using UnityEngine;

/// <summary>
/// Gère les sons du personnage : saut, chute (sweep fall) et escalade.
/// À placer sur le même GameObject que l'Animator.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    private const float JumpSoundChance = 0.05f;
    private const float FallSoundChance = 0.10f;
    private const float ClimbSoundChance = 0.10f;

    [Header("Saut")]
    [SerializeField] private AudioClip[] _jumpClips;
    [SerializeField] [Range(0f, 1f)] private float _jumpVolume = 1f;

    [Header("Chute (Sweep Fall)")]
    [SerializeField] private AudioClip[] _fallClips;
    [SerializeField] [Range(0f, 1f)] private float _fallVolume = 1f;

    [Header("Escalade")]
    [SerializeField] private AudioClip[] _climbClips;
    [SerializeField] [Range(0f, 1f)] private float _climbVolume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        PlayerEvents.OnJumpExecuted += HandleJumpExecuted;
        PlayerEvents.OnSweepFallAnimStarted += HandleSweepFallAnimStarted;
        PlayerEvents.OnClimbStarted += HandleClimbStarted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnJumpExecuted -= HandleJumpExecuted;
        PlayerEvents.OnSweepFallAnimStarted -= HandleSweepFallAnimStarted;
        PlayerEvents.OnClimbStarted -= HandleClimbStarted;
    }

    private void HandleJumpExecuted()
    {
        if (Random.value <= JumpSoundChance)
            PlayRandom(_jumpClips, _jumpVolume);
    }

    private void HandleSweepFallAnimStarted()
    {
        if (Random.value <= FallSoundChance)
            PlayRandom(_fallClips, _fallVolume);
    }

    private void HandleClimbStarted()
    {
        if (Random.value <= ClimbSoundChance)
            PlayRandom(_climbClips, _climbVolume);
    }

    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        _audioSource.PlayOneShot(clip, volume);
    }
}
