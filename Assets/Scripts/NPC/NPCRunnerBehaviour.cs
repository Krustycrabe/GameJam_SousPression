using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Behaviour composite : délègue à un behaviour de base et bascule en mode agressif
/// quand le joueur entre dans la zone de détection.
/// </summary>
public class NPCRunnerBehaviour : MonoBehaviour, INPCBehaviour
{
    private enum Phase { Base, ChasingPlayer, WaitingAfterPush, SeekingBriefcase, RunningWithBriefcase }

    [Header("Behaviour de base")]
    [Tooltip("NPCRandomWalkBehaviour, NPCPatrolBehaviour, etc.")]
    [SerializeField] private MonoBehaviour _baseBehaviourComponent;

    [Header("Détection")]
    [SerializeField] private float _detectionRadius = 6f;
    [SerializeField] private string _playerTag = "Player";

    [Header("Mode agressif")]
    [SerializeField] private float _runSpeed = 5f;
    [SerializeField] private float _pushDistance = 1.5f;
    [Tooltip("Durée d'attente après un push avant de reprendre la chasse.")]
    [SerializeField] private float _pushCooldown = 3.5f;

    [Header("Fuite avec malette")]
    [SerializeField] private float _randomRunRadius = 12f;
    [SerializeField] private float _waypointTolerance = 0.5f;

    [Header("Retour après ragdoll")]
    [Tooltip("Si vrai, reprend le behaviour de base. Sinon, reste en mode agressif.")]
    [SerializeField] private bool _returnToBaseAfterRagdoll = true;

    private INPCBehaviour _base;
    private NPCController _npc;
    private NPCBriefcaseController _briefcaseController;
    private Transform _playerTransform;
    private Phase _phase;
    private float _waitTimer;
    private Vector3 _origin;
    private const float ChaseThrottle = 0.15f;
    private float _lastChaseUpdate;

    // ── INPCBehaviour ────────────────────────────────────────────────────────

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _briefcaseController = npc.GetComponent<NPCBriefcaseController>();
        _origin = npc.transform.position;
        _base = _baseBehaviourComponent as INPCBehaviour;

        if (_base == null)
            Debug.LogWarning("[NPCRunnerBehaviour] _baseBehaviourComponent n'implémente pas INPCBehaviour.", this);

        GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning($"[NPCRunnerBehaviour] Tag '{_playerTag}' introuvable.", this);

        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;

        if (_briefcaseController != null && _briefcaseController.HasBriefcase)
            EnterPhase(Phase.RunningWithBriefcase);
        else if (!_returnToBaseAfterRagdoll)
            EnterPhase(Phase.ChasingPlayer);
        else
            EnterPhase(Phase.Base);
    }

    public void OnExit()
    {
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;

        if (_phase == Phase.Base)
            _base?.OnExit();
        else
            _npc?.Agent.ResetPath();
    }

    public void OnTick()
    {
        switch (_phase)
        {
            case Phase.Base: TickBase(); break;
            case Phase.ChasingPlayer: TickChasing(); break;
            case Phase.WaitingAfterPush: TickWaiting(); break;
            case Phase.SeekingBriefcase: TickSeekingBriefcase(); break;
            case Phase.RunningWithBriefcase: TickRunningWithBriefcase(); break;
        }
    }

    // ── Phases ───────────────────────────────────────────────────────────────

    private void TickBase()
    {
        _base?.OnTick();

        if (_playerTransform == null) return;
        float dist = Vector3.Distance(_npc.transform.position, _playerTransform.position);
        if (dist <= _detectionRadius)
            EnterPhase(Phase.ChasingPlayer);
    }

    private void TickChasing()
    {
        if (_playerTransform == null) return;

        // Rotation manuelle vers le joueur
        Vector3 toPlayer = _playerTransform.position - _npc.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            _npc.transform.rotation = Quaternion.Slerp(_npc.transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        float dist = Vector3.Distance(_npc.transform.position, _playerTransform.position);
        if (dist <= _pushDistance)
        {
            ExecutePush();
            return;
        }

        if (Time.time - _lastChaseUpdate < ChaseThrottle) return;
        _lastChaseUpdate = Time.time;
        _npc.Agent.SetDestination(_playerTransform.position);
    }

    private void TickWaiting()
    {
        _waitTimer -= Time.deltaTime;
        if (_waitTimer <= 0f)
            EnterPhase(Phase.ChasingPlayer);
    }

    private void TickSeekingBriefcase()
    {
        if (_briefcaseController == null) return;

        if (_briefcaseController.TryPickup())
        {
            EnterPhase(Phase.RunningWithBriefcase);
            return;
        }

        BriefcaseItem nearest = FindNearestBriefcase();
        if (nearest != null)
            _npc.Agent.SetDestination(nearest.transform.position);
    }

    private void TickRunningWithBriefcase()
    {
        if (!_npc.Agent.pathPending && _npc.Agent.remainingDistance <= _waypointTolerance)
            PickRandomDestination();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    private void ExecutePush()
    {
        _npc.Agent.ResetPath();

        Vector3 dir = (_playerTransform.position - _npc.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            _npc.transform.rotation = Quaternion.LookRotation(dir.normalized);

        _npc.AnimController.TriggerPush();

        // RaiseSweepFallStarted est synchrone : HandleBriefcaseDropped peut déjà
        // avoir transitionné vers SeekingBriefcase avant la ligne suivante.
        PlayerEvents.RaiseSweepFallStarted();

        // Si on est déjà en SeekingBriefcase (via HandleBriefcaseDropped), ne pas écraser.
        if (_phase == Phase.SeekingBriefcase) return;

        // Cas 2 : la malette était déjà au sol avant le push — vérifie activement.
        if (_briefcaseController != null && FindNearestBriefcase() != null)
        {
            EnterPhase(Phase.SeekingBriefcase);
            return;
        }

        // Aucune malette disponible : attend le cooldown puis rechasse.
        EnterPhase(Phase.WaitingAfterPush);
    }

    private void HandleBriefcaseDropped()
    {
        // Réagit depuis n'importe quelle phase sauf si on porte déjà la malette
        if (_phase == Phase.RunningWithBriefcase) return;
        if (_briefcaseController == null || _briefcaseController.HasBriefcase) return;

        EnterPhase(Phase.SeekingBriefcase);
    }


    // ── Transitions ──────────────────────────────────────────────────────────

    private void EnterPhase(Phase phase)
    {
        Phase previous = _phase;
        _phase = phase;

        if (previous == Phase.Base && phase != Phase.Base)
            _base?.OnExit();

        switch (phase)
        {
            case Phase.Base:
                _npc.Agent.stoppingDistance = 0f;
                _npc.Agent.updateRotation = true;
                _base?.OnEnter(_npc);
                break;

            case Phase.ChasingPlayer:
                _npc.Agent.isStopped = false;
                _npc.Agent.speed = _runSpeed;
                _npc.Agent.stoppingDistance = _pushDistance * 0.6f;
                _npc.Agent.updateRotation = false;
                break;

            case Phase.WaitingAfterPush:
                // Stoppe sur place le temps du cooldown
                _npc.Agent.isStopped = true;
                _npc.Agent.ResetPath();
                _waitTimer = _pushCooldown;
                break;

            case Phase.SeekingBriefcase:
                _npc.Agent.isStopped = false;
                _npc.Agent.speed = _runSpeed;
                _npc.Agent.stoppingDistance = 0f;
                _npc.Agent.updateRotation = true;
                break;

            case Phase.RunningWithBriefcase:
                _npc.Agent.isStopped = false;
                _npc.Agent.speed = _runSpeed;
                _npc.Agent.stoppingDistance = 0f;
                _npc.Agent.updateRotation = true;
                PickRandomDestination();
                break;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void PickRandomDestination()
    {
        Vector3 randomPoint = _origin + Random.insideUnitSphere * _randomRunRadius;
        randomPoint.y = _origin.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _randomRunRadius, NavMesh.AllAreas))
            _npc.Agent.SetDestination(hit.position);
    }

    private BriefcaseItem FindNearestBriefcase()
    {
        Collider[] colliders = Physics.OverlapSphere(_npc.transform.position, 30f);
        BriefcaseItem nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider col in colliders)
        {
            BriefcaseItem item = col.GetComponentInParent<BriefcaseItem>();
            if (item == null || item.IsHeld) continue;

            float dist = Vector3.Distance(_npc.transform.position, item.transform.position);
            if (dist < nearestDist) { nearestDist = dist; nearest = item; }
        }

        return nearest;
    }
}
