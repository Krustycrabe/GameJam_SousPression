using System.Collections;
using UnityEngine;

/// <summary>
/// Gère le cycle complet de l'ascenseur.
/// La détection du player est déléguée à un enfant ElevatorTriggerZone.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    private enum ElevatorState
    {
        IdleAtBottom,
        Opening,
        OpenAtBottom,
        ClosingAtBottom,
        MovingUp,
        OpenAtTop,
        ClosingAtTop,
        MovingDown
    }

    private const float PositionThreshold = 0.001f;
    private const float BellAnticipation = 0.5f;

    [Header("Portes")]
    [SerializeField] private Transform _doorLeft;
    [SerializeField] private Transform _doorRight;
    [SerializeField] private Vector3 _doorLeftOpenOffset = new Vector3(-1.5f, 0f, 0f);
    [SerializeField] private Vector3 _doorRightOpenOffset = new Vector3(1.5f, 0f, 0f);
    [SerializeField] private float _doorSpeed = 2f;

    [Header("Timing")]
    [SerializeField] private float _closeDelay = 1.5f;
    [SerializeField] private float _moveDelay = 1f;

    [Header("Déplacement")]
    [SerializeField] private Transform _destinationMarker;
    [SerializeField] private float _elevatorSpeed = 3f;

    [Header("Audio - SFX")]
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] private AudioClip _doorOpenClip;
    [SerializeField] private AudioClip _doorCloseClip;
    [SerializeField] private AudioClip _bellClip;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolumeDeparture = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolumeArrival = 0.5f;

    [Header("Audio - Loop moteur")]
    [SerializeField] private AudioSource _loopAudioSource;
    [SerializeField] private AudioClip _movingLoopClip;
    [SerializeField] [Range(0f, 1f)] private float _loopVolume = 0.25f;

    [Header("Audio - Musique")]
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioClip _musicClip;
    [SerializeField] [Range(0f, 1f)] private float _musicVolume = 1f;
    [SerializeField] private float _musicFadeOutDuration = 1f;

    private ElevatorState _state = ElevatorState.IdleAtBottom;
    private Vector3 _bottomPosition;
    private Vector3 _topPosition;
    private Vector3 _doorLeftClosedLocalPos;
    private Vector3 _doorRightClosedLocalPos;
    private Coroutine _sequenceCoroutine;
    private Coroutine _fadeCoroutine;
    private Transform _playerTransform;

    private void Start()
    {
        _bottomPosition = transform.position;
        _topPosition = _destinationMarker.position;
        _doorLeftClosedLocalPos = _doorLeft.localPosition;
        _doorRightClosedLocalPos = _doorRight.localPosition;
    }

    /// <summary>Appelé par ElevatorTriggerZone quand le player entre.</summary>
    public void OnPlayerEntered(Transform player)
    {
        _playerTransform = player;

        if (_state == ElevatorState.IdleAtBottom)
        {
            if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = StartCoroutine(AscendSequence());
        }
    }

    /// <summary>Appelé par ElevatorTriggerZone quand le player sort.</summary>
    public void OnPlayerExited()
    {
        if (_state == ElevatorState.OpenAtTop)
        {
            StartMusicFadeOut();

            if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = StartCoroutine(DescendSequence());
        }
    }

    private IEnumerator AscendSequence()
    {
        _state = ElevatorState.Opening;
        PlaySfx(_doorOpenClip, _sfxVolumeDeparture);
        yield return MoveDoors(open: true);

        _state = ElevatorState.OpenAtBottom;
        yield return new WaitForSeconds(_closeDelay);

        _state = ElevatorState.ClosingAtBottom;
        PlaySfx(_doorCloseClip, _sfxVolumeDeparture);
        yield return MoveDoors(open: false);

        // Sonnette 0.5s avant le départ
        float waitBeforeBell = _moveDelay - BellAnticipation;
        if (waitBeforeBell > 0f)
            yield return new WaitForSeconds(waitBeforeBell);

        PlaySfx(_bellClip, _sfxVolumeDeparture);
        yield return new WaitForSeconds(Mathf.Min(BellAnticipation, _moveDelay));

        _state = ElevatorState.MovingUp;
        SetPlayerParented(true);
        StartMovingLoop();
        StartMusic();
        yield return MoveElevator(_topPosition);
        StopMovingLoop();
        SetPlayerParented(false);

        _state = ElevatorState.Opening;
        PlaySfx(_doorOpenClip, _sfxVolumeArrival);
        yield return MoveDoors(open: true);

        _state = ElevatorState.OpenAtTop;
    }

    private IEnumerator DescendSequence()
    {
        yield return new WaitForSeconds(_closeDelay);

        _state = ElevatorState.ClosingAtTop;
        PlaySfx(_doorCloseClip, _sfxVolumeArrival);
        yield return MoveDoors(open: false);

        _state = ElevatorState.MovingDown;
        // Pas de loop moteur ni de musique à la descente — ascenseur vide
        yield return MoveElevator(_bottomPosition);

        _state = ElevatorState.IdleAtBottom;
    }

    private IEnumerator MoveDoors(bool open)
    {
        Vector3 leftTarget = _doorLeftClosedLocalPos + (open ? _doorLeftOpenOffset : Vector3.zero);
        Vector3 rightTarget = _doorRightClosedLocalPos + (open ? _doorRightOpenOffset : Vector3.zero);

        while (Vector3.Distance(_doorLeft.localPosition, leftTarget) > PositionThreshold)
        {
            _doorLeft.localPosition = Vector3.MoveTowards(
                _doorLeft.localPosition, leftTarget, _doorSpeed * Time.deltaTime);
            _doorRight.localPosition = Vector3.MoveTowards(
                _doorRight.localPosition, rightTarget, _doorSpeed * Time.deltaTime);
            yield return null;
        }

        _doorLeft.localPosition = leftTarget;
        _doorRight.localPosition = rightTarget;
    }

    private IEnumerator MoveElevator(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > PositionThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, _elevatorSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (_sfxAudioSource == null || clip == null) return;
        _sfxAudioSource.PlayOneShot(clip, volume);
    }

    private void StartMovingLoop()
    {
        if (_loopAudioSource == null || _movingLoopClip == null) return;
        _loopAudioSource.clip = _movingLoopClip;
        _loopAudioSource.volume = _loopVolume;
        _loopAudioSource.loop = true;
        _loopAudioSource.Play();
    }

    private void StopMovingLoop()
    {
        if (_loopAudioSource == null) return;
        _loopAudioSource.Stop();
        _loopAudioSource.loop = false;
        _loopAudioSource.clip = null;
    }

    private void StartMusic()
    {
        if (_musicAudioSource == null || _musicClip == null) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        _musicAudioSource.clip = _musicClip;
        _musicAudioSource.volume = _musicVolume;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();
    }

    private void StartMusicFadeOut()
    {
        if (_musicAudioSource == null || !_musicAudioSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = _musicAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < _musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / _musicFadeOutDuration);
            yield return null;
        }

        _musicAudioSource.Stop();
        _musicAudioSource.volume = _musicVolume;
        _musicAudioSource.clip = null;
    }

    /// <summary>Parenté le player à la cabine pendant le déplacement.</summary>
    private void SetPlayerParented(bool parented)
    {
        if (_playerTransform == null) return;
        _playerTransform.SetParent(parented ? transform : null);
    }
}
