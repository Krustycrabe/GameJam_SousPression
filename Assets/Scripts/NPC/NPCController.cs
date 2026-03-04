using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCAnimationController))]
[RequireComponent(typeof(NPCRagdollController))]
public class NPCController : MonoBehaviour
{
    public enum State { Behaviour, KnockedDown, GettingUp, Chasing, Pushing, Airborne }

    [Header("Knockdown & Chase")]
    [SerializeField] private float _knockDownDuration = 2f;
    [SerializeField] private float _getUpDuration = 1.5f;
    [SerializeField] private float _chaseSpeed = 4f;
    [SerializeField] private float _pushDistance = 1.5f;
    [SerializeField] private float _pushCooldown = 3f;

    [Header("Trampoline")]
    [SerializeField] private float _gravityScale = 2f;

    [Header("Références")]
    [SerializeField] private string _playerTag = "Player";

    private const float GravityConstant = 9.81f;
    private const float ChaseUpdateInterval = 0.2f;
    private const float ChaseRotationSpeed = 10f;

    private State _state = State.Behaviour;
    private State _stateBeforeAirborne = State.Behaviour;
    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private NPCAnimationController _animController;
    private NPCRagdollController _ragdollController;
    private INPCBehaviour _behaviour;
    private Transform _playerTransform;
    private float _defaultSpeed;
    private float _defaultBaseOffset;
    private float _lastChaseUpdateTime;
    private bool _pushInProgress;

    // Gestion verticale manuelle via baseOffset — NavMeshAgent toujours actif
    private float _verticalVelocity;
    private float _airborneHeight;

    public State CurrentState => _state;
    public NavMeshAgent Agent => _agent;
    public NPCAnimationController AnimController => _animController;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _animController = GetComponent<NPCAnimationController>();
        _ragdollController = GetComponent<NPCRagdollController>();
        _behaviour = GetComponent<INPCBehaviour>();
        _defaultSpeed = _agent.speed;
        _defaultBaseOffset = _agent.baseOffset;

        // Force le Rigidbody en kinematic — le NavMeshAgent gère tout le mouvement
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning($"[NPCController] Aucun GameObject avec le tag '{_playerTag}' trouvé.", this);

        _behaviour?.OnEnter(this);
    }

    private void Update()
    {
        UpdateVertical();

        switch (_state)
        {
            case State.Behaviour:
            case State.Airborne:
                _behaviour?.OnTick();
                break;
            case State.Chasing:
                UpdateChase();
                break;
        }

        UpdateAnimation();
    }

    // ── Vertical (trampoline via baseOffset) ────────────────────────────────

    private void UpdateVertical()
    {
        if (_airborneHeight <= 0f && _verticalVelocity <= 0f) return;

        _verticalVelocity -= GravityConstant * _gravityScale * Time.deltaTime;
        _airborneHeight += _verticalVelocity * Time.deltaTime;

        if (_airborneHeight <= 0f)
        {
            _airborneHeight = 0f;
            _verticalVelocity = 0f;
            _agent.baseOffset = _defaultBaseOffset;
            _animController.SetGrounded(true);

            if (_state == State.Airborne)
            {
                _state = _stateBeforeAirborne;
                if (_state == State.Behaviour)
                    _behaviour?.OnEnter(this);
            }
            return;
        }

        _agent.baseOffset = _defaultBaseOffset + _airborneHeight;
    }

    // ── Impacts externes ─────────────────────────────────────────────────────

    /// <summary>Impact de malette — déclenche le ragdoll.</summary>
    public void OnHit(Vector3 force)
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        ResetVertical();
        StopAllCoroutines();
        _pushInProgress = false;
        StartCoroutine(KnockDownSequence(force));
    }

    /// <summary>Trampoline — saut via baseOffset, pas de ragdoll.</summary>
    public void OnTrampolineHit(float verticalForce)
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        if (_state != State.Airborne)
            _stateBeforeAirborne = _state;

        _state = State.Airborne;
        _verticalVelocity = verticalForce;
        _animController.SetGrounded(false);
    }

    // ── Séquences ────────────────────────────────────────────────────────────

    private IEnumerator KnockDownSequence(Vector3 force)
    {
        _state = State.KnockedDown;
        _behaviour?.OnExit();
        SetAgentActive(false);
        _ragdollController.EnableRagdoll(force);

        yield return new WaitForSeconds(_knockDownDuration);

        _ragdollController.DisableRagdoll();
        _state = State.GettingUp;
        _animController.TriggerGetUp();
        SetAgentActive(true);

        yield return new WaitForSeconds(_getUpDuration);

        // Passe en chase uniquement si le joueur est connu
        if (_playerTransform != null)
        {
            EnterChaseMode();
        }
        else
        {
            EnterBehaviourMode();
        }
    }

    private IEnumerator PushSequence()
    {
        _pushInProgress = true;
        _state = State.Pushing;
        _agent.ResetPath();
        _agent.updateRotation = true;

        // Oriente instantanément vers le joueur
        if (_playerTransform != null)
        {
            Vector3 dir = (_playerTransform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        _animController.TriggerPush();
        yield return new WaitForSeconds(0.3f);

        PlayerEvents.RaiseSweepFallStarted();

        yield return new WaitForSeconds(_pushCooldown);

        _pushInProgress = false;
        EnterBehaviourMode();
    }

    // ── Chase ────────────────────────────────────────────────────────────────

    private void UpdateChase()
    {
        if (_playerTransform == null || _pushInProgress) return;

        // Rotation manuelle vers le joueur
        Vector3 toPlayer = _playerTransform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, ChaseRotationSpeed * Time.deltaTime);
        }

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist <= _pushDistance)
        {
            StartCoroutine(PushSequence());
            return;
        }

        // Throttle SetDestination pour éviter les recalculs trop fréquents
        if (Time.time - _lastChaseUpdateTime < ChaseUpdateInterval) return;
        _lastChaseUpdateTime = Time.time;
        _agent.SetDestination(_playerTransform.position);
    }

    // ── Helpers d'état ───────────────────────────────────────────────────────

    private void EnterChaseMode()
    {
        _state = State.Chasing;
        _agent.speed = _chaseSpeed;
        _agent.stoppingDistance = _pushDistance * 0.6f;
        _agent.updateRotation = false; // rotation manuelle dans UpdateChase
    }

    private void EnterBehaviourMode()
    {
        _state = State.Behaviour;
        _agent.speed = _defaultSpeed;
        _agent.stoppingDistance = 0f;
        _agent.updateRotation = true;
        _behaviour?.OnEnter(this);
    }

    private void SetAgentActive(bool active)
    {
        if (_agent != null) _agent.enabled = active;
    }

    private void ResetVertical()
    {
        _airborneHeight = 0f;
        _verticalVelocity = 0f;
        if (_agent != null) _agent.baseOffset = _defaultBaseOffset;
    }

    // ── Animation ────────────────────────────────────────────────────────────

    private void UpdateAnimation()
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        float speed = _agent.enabled && _agent.isOnNavMesh
            ? Mathf.Clamp01(_agent.velocity.magnitude / Mathf.Max(_agent.speed, 0.01f))
            : 0f;

        _animController.SetSpeed(speed);
    }
}
