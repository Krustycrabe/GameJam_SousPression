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

    private State _state = State.Behaviour;
    private State _stateBeforeAirborne = State.Behaviour;
    private NavMeshAgent _agent;
    private NPCAnimationController _animController;
    private NPCRagdollController _ragdollController;
    private INPCBehaviour _behaviour;
    private Transform _playerTransform;
    private float _defaultSpeed;
    private float _defaultBaseOffset;

    // Gestion verticale manuelle — sans Rigidbody ni désactivation de l'agent
    private float _verticalVelocity = 0f;
    private float _airborneHeight = 0f;

    public State CurrentState => _state;
    public NavMeshAgent Agent => _agent;
    public NPCAnimationController AnimController => _animController;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<NPCAnimationController>();
        _ragdollController = GetComponent<NPCRagdollController>();
        _behaviour = GetComponent<INPCBehaviour>();
        _defaultSpeed = _agent.speed;
        _defaultBaseOffset = _agent.baseOffset;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
        if (player) _playerTransform = player.transform;

        _behaviour?.OnEnter(this);
    }

    private void Update()
    {
        UpdateVertical();

        switch (_state)
        {
            // Le patrol tick pendant le vol — l'agent est toujours actif
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

    /// <summary>
    /// Gère la gravité et la hauteur verticale via baseOffset.
    /// Le NavMeshAgent reste actif — la patrol continue pendant le vol.
    /// </summary>
    private void UpdateVertical()
    {
        if (_airborneHeight <= 0f && _verticalVelocity <= 0f) return;

        _verticalVelocity -= GravityConstant * _gravityScale * Time.deltaTime;
        _airborneHeight += _verticalVelocity * Time.deltaTime;

        if (_airborneHeight <= 0f)
        {
            // Atterrissage
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

    /// <summary>Point d'entrée pour tout impact externe (malette, etc.).</summary>
    public void OnHit(Vector3 force)
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        // Annule un saut en cours avant le ragdoll
        _airborneHeight = 0f;
        _verticalVelocity = 0f;
        _agent.baseOffset = _defaultBaseOffset;

        StartCoroutine(KnockDownSequence(force));
    }

    /// <summary>Lance le NPC en l'air — NavMeshAgent actif, patrol ininterrompue.</summary>
    public void OnTrampolineHit(float verticalForce)
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        // Mémorise l'état précédent uniquement si pas déjà en vol
        if (_state != State.Airborne)
            _stateBeforeAirborne = _state;

        _state = State.Airborne;
        _verticalVelocity = verticalForce; // vitesse initiale en m/s
        _animController.SetGrounded(false);
    }

    private IEnumerator KnockDownSequence(Vector3 force)
    {
        _state = State.KnockedDown;
        _behaviour?.OnExit();

        _agent.enabled = false;
        _ragdollController.EnableRagdoll(force);

        yield return new WaitForSeconds(_knockDownDuration);

        _ragdollController.DisableRagdoll();
        _state = State.GettingUp;
        _animController.TriggerGetUp();
        _agent.enabled = true;

        yield return new WaitForSeconds(_getUpDuration);

        _state = State.Chasing;
        _agent.speed = _chaseSpeed;
    }

    private void UpdateChase()
    {
        if (_playerTransform == null) return;

        if (Vector3.Distance(transform.position, _playerTransform.position) <= _pushDistance)
            StartCoroutine(PushSequence());
        else
            _agent.SetDestination(_playerTransform.position);
    }

    private IEnumerator PushSequence()
    {
        _state = State.Pushing;
        _agent.ResetPath();

        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        _animController.TriggerPush();
        yield return new WaitForSeconds(0.3f);

        PlayerEvents.RaiseSweepFallStarted();

        yield return new WaitForSeconds(_pushCooldown);

        _state = State.Behaviour;
        _agent.speed = _defaultSpeed;
        _behaviour?.OnEnter(this);
    }

    private void UpdateAnimation()
    {
        if (_state is State.KnockedDown or State.GettingUp) return;

        float speed = _agent.enabled && _agent.isOnNavMesh
            ? Mathf.Clamp01(_agent.velocity.magnitude / Mathf.Max(_agent.speed, 0.01f))
            : 0f;

        _animController.SetSpeed(speed);
        // SetGrounded est géré exclusivement par UpdateVertical
    }
}
