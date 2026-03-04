using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrampolineZone : MonoBehaviour
{
    [SerializeField] private float _verticalForce = 12f;
    [SerializeField] private float _horizontalForce = 6f;
    [SerializeField] private string _playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _bounceClips;
    [SerializeField] [Range(0f, 1f)] private float _bounceVolume = 1f;

    private bool _playerInZone;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnEnable() => PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
    private void OnDisable() => PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;

    private void OnTriggerEnter(Collider other)
    {
        // Player
        if (other.CompareTag(_playerTag))
        {
            _playerInZone = true;
            LaunchPlayer();
            return;
        }

        // NPC — nécessite un Rigidbody kinematic sur le root du NPC
        NPCController npc = other.GetComponentInParent<NPCController>();
        if (npc != null)
        {
            npc.OnTrampolineHit(_verticalForce);
            return;
        }

        // Malette en vol libre
        BriefcaseItem briefcase = other.GetComponentInParent<BriefcaseItem>();
        if (briefcase != null && !briefcase.IsHeld)
        {
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            if (rb != null)
                rb.AddForce(Vector3.up * _verticalForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_playerTag)) _playerInZone = false;
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        if (isGrounded && _playerInZone) LaunchPlayer();
    }

    private void LaunchPlayer()
    {
        PlayerEvents.RaiseTrampolineBounce(_verticalForce, _horizontalForce);
        PlayRandomBounceSound();
    }

    private void PlayRandomBounceSound()
    {
        if (_audioSource == null || _bounceClips == null || _bounceClips.Length == 0) return;

        AudioClip clip = _bounceClips[Random.Range(0, _bounceClips.Length)];
        _audioSource.PlayOneShot(clip, _bounceVolume);
    }
}
