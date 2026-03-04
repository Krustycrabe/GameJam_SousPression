using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerClimbController : MonoBehaviour
{
    [Header("Détection de l'arête")]
    [SerializeField] private string _climbableTag = "Climbable";
    [SerializeField] private float _detectHeight = 1.4f;
    [SerializeField] private float _climbReach = 0.9f;
    [SerializeField] private float _minLedgeHeight = 0.8f;
    [SerializeField] private float _maxLedgeHeight = 2.3f;

    [Header("Positionnement")]
    [SerializeField] private float _hangOffset = 1.7f;
    [SerializeField] private float _wallOffset = 0.3f;

    [Header("Timing")]
    [SerializeField] private float _climbDuration = 1.5f;

    [Header("Forward Boost")]
    [SerializeField] private float _forwardBoostDuration = 0.5f;
    [SerializeField] private float _forwardBoostDistance = 0.3f;

    private CharacterController _characterController;
    private Animator _animator;
    private bool _isGrounded;
    private bool _isClimbing;
    private bool _climbAnimationEnded;
    private bool _isHoldingBriefcase;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Debug.LogError("[ClimbController] OnEnable — subscription OK");
        PlayerEvents.OnJumpInput += HandleJumpInput;
        PlayerEvents.OnGroundedChanged += HandleGroundedChanged;
        PlayerEvents.OnClimbAnimationEnd += HandleClimbAnimationEnd;
        PlayerEvents.OnBriefcasePickedUp += HandleBriefcasePickedUp;
        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;
    }

    private void OnDisable()
    {
        PlayerEvents.OnJumpInput -= HandleJumpInput;
        PlayerEvents.OnGroundedChanged -= HandleGroundedChanged;
        PlayerEvents.OnClimbAnimationEnd -= HandleClimbAnimationEnd;
        PlayerEvents.OnBriefcasePickedUp -= HandleBriefcasePickedUp; // ← nouveau
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;  // ← nouveau
    }

    private void HandleGroundedChanged(bool isGrounded) => _isGrounded = isGrounded;

    /// <summary>
    /// Appelé par ClimbExitBehaviour exactement quand le state Climb quitte l'Animator,
    /// AVANT que le root motion de Locomotion ne soit appliqué ce frame.
    /// </summary>
    private void HandleClimbAnimationEnd()
    {
        _animator.applyRootMotion = false;
        _climbAnimationEnded = true;
    }

    private void HandleBriefcasePickedUp(Transform _) => _isHoldingBriefcase = true;
    private void HandleBriefcaseDropped() => _isHoldingBriefcase = false;

    private void HandleJumpInput()
    {
        Debug.LogWarning($"[Climb] JumpInput reçu — isGrounded:{_isGrounded} | isClimbing:{_isClimbing} | holdingBriefcase:{_isHoldingBriefcase}");

        if (_isGrounded) { Debug.LogWarning("[Climb] BLOQUÉ : joueur au sol"); return; }
        if (_isClimbing) { Debug.LogWarning("[Climb] BLOQUÉ : climb déjà en cours"); return; }
        if (_isHoldingBriefcase) { Debug.LogWarning("[Climb] BLOQUÉ : mallette en main"); return; }

        if (TryDetectLedge(out Vector3 hangPosition, out Quaternion hangRotation))
        {
            Debug.LogWarning($"[Climb] Arête détectée → ClimbSequence à {hangPosition}");
            StartCoroutine(ClimbSequence(hangPosition, hangRotation));
        }
        else
        {
            Debug.LogWarning("[Climb] TryDetectLedge échoué");
        }
    }

    private bool TryDetectLedge(out Vector3 hangPosition, out Quaternion hangRotation)
    {
        hangPosition = Vector3.zero;
        hangRotation = Quaternion.identity;

        Vector3 chestOrigin = transform.position + Vector3.up * _detectHeight;
        Debug.DrawRay(chestOrigin, transform.forward * _climbReach, Color.red, 2f);

        // ── Raycast mur — ignore les colliders du joueur lui-même ───────────────
        RaycastHit[] hits = Physics.RaycastAll(chestOrigin, transform.forward, _climbReach);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit wallHit = default;
        bool foundWall = false;
        foreach (RaycastHit h in hits)
        {
            if (h.transform.IsChildOf(transform) || h.transform == transform) continue; // ignore self
            wallHit = h;
            foundWall = true;
            break;
        }

        if (!foundWall)
        {
            Debug.LogWarning($"[Climb] Raycast mur raté — portée:{_climbReach}m depuis Y:{chestOrigin.y:F2}");
            return false;
        }

        Debug.LogWarning($"[Climb] Mur touché : '{wallHit.collider.name}' tag:'{wallHit.collider.tag}'");

        if (!wallHit.collider.CompareTag(_climbableTag))
        {
            Debug.LogWarning($"[Climb] Tag incorrect — attendu:'{_climbableTag}' reçu:'{wallHit.collider.tag}'");
            return false;
        }
        // ── Suite inchangée ─────────────────────────────────────────────────────

        Vector3 aboveOrigin = new Vector3(
            wallHit.point.x - wallHit.normal.x * 0.1f,
            transform.position.y + _maxLedgeHeight + 0.5f,
            wallHit.point.z - wallHit.normal.z * 0.1f
        );

        Debug.DrawRay(aboveOrigin, Vector3.down * (_maxLedgeHeight + 1f), Color.blue, 2f);

        if (!Physics.Raycast(aboveOrigin, Vector3.down, out RaycastHit ledgeHit, _maxLedgeHeight + 1f))
        {
            Debug.LogWarning("[Climb] Raycast arête (vers le bas) raté");
            return false;
        }

        float ledgeLocalY = ledgeHit.point.y - transform.position.y;
        Debug.LogWarning($"[Climb] Hauteur arête : {ledgeLocalY:F2}m (min:{_minLedgeHeight} max:{_maxLedgeHeight})");

        if (ledgeLocalY < _minLedgeHeight || ledgeLocalY > _maxLedgeHeight)
        {
            Debug.LogWarning("[Climb] Hauteur hors plage !");
            return false;
        }

        Vector3 wallNormal = wallHit.normal;
        hangRotation = Quaternion.LookRotation(-wallNormal);
        hangPosition = new Vector3(
            wallHit.point.x + wallNormal.x * _wallOffset,
            ledgeHit.point.y - _hangOffset,
            wallHit.point.z + wallNormal.z * _wallOffset
        );

        return true;
    }



    private IEnumerator ClimbSequence(Vector3 hangPosition, Quaternion hangRotation)
    {
        _isClimbing = true;
        _climbAnimationEnded = false;
        _characterController.enabled = false;
        _animator.applyRootMotion = true;

        transform.position = hangPosition;
        transform.rotation = hangRotation;

        PlayerEvents.RaiseClimbStarted();

        // Phase 1 — root motion seul
        float phase1 = Mathf.Max(0f, _climbDuration - _forwardBoostDuration);
        yield return new WaitForSeconds(phase1);

        // Phase 2 — root motion + forward boost
        float boostElapsed = 0f;
        float prevT = 0f;

        while (boostElapsed < _forwardBoostDuration && !_climbAnimationEnded)
        {
            boostElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(boostElapsed / _forwardBoostDuration));
            float frameDelta = (t - prevT) * _forwardBoostDistance;
            transform.position += transform.forward * frameDelta;
            prevT = t;
            yield return null;
        }

        // Attend le signal exact de ClimbExitBehaviour si l'anim n'est pas encore terminée
        yield return new WaitUntil(() => _climbAnimationEnded);

        // applyRootMotion = false déjà fait dans HandleClimbAnimationEnd
        SnapToGround();

        _characterController.enabled = true;

        // 2 frames pour que le CC détecte isGrounded = true avant ApplyGravity
        yield return null;
        yield return null;

        _isClimbing = false;
        PlayerEvents.RaiseClimbCompleted();
    }

    /// <summary>Snap le transform exactement sur la surface sous les pieds.</summary>
    private void SnapToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y,
                transform.position.z
            );
        }
    }
}
