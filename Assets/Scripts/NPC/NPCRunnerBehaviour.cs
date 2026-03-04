using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Behaviour NPC agressif : détecte le joueur, le pousse, ramasse la malette et fuit.
/// Compatible avec les autres behaviours (patrol, dance, etc.) sur d'autres NPCs.
/// </summary>
public class NPCRunnerBehaviour : MonoBehaviour, INPCBehaviour
{
    private enum Phase { Watching, ChasingPlayer, SeekingBriefcase, RunningWithBriefcase }

    [Header("Détection")]
    [SerializeField] private float _detectionRadius = 6f;
    [SerializeField] private string _playerTag = "Player";

    [Header("Mouvement")]
    [SerializeField] private float _runSpeed = 5f;
    [SerializeField] private float _pushDistance = 1.5f;
    [SerializeField] private float _pushCooldown = 2f;

    [Header("Fuite avec malette")]
    [SerializeField] private float _randomRunRadius = 12f;
    [SerializeField] private float _waypointTolerance = 0.5f;

    [Header("Comportement après ragdoll")]
    [Tooltip("Si vrai, retourne en Watching après avoir été ragdollé. Sinon, reprend la chasse.")]
    [SerializeField] private bool _returnToWatchingAfterRagdoll = true;

    private NPCController _npc;
    private NPCBriefcaseController _briefcaseController;
    private Transform _playerTransform;
    private Phase _phase;
    private float _lastPushTime = -999f;
    private Vector3 _origin;

    // ── Interface INPCBehaviour ──────────────────────────────────────────────

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _briefcaseController = npc.GetComponent<NPCBriefcaseController>();
        _origin = npc.transform.position;

        GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
        if (player != null) _playerTransform = player.transform;

        PlayerEvents.OnBriefcaseDropped += HandleBriefcaseDropped;

        // Détermine la phase de départ selon l'état
        if (_briefcaseController != null && _briefcaseController.HasBriefcase)
            EnterPhase(Phase.RunningWithBriefcase);
        else if (!_returnToWatchingAfterRagdoll)
            EnterPhase(Phase.ChasingPlayer);
        else
            EnterPhase(Phase.Watching);
    }

    public void OnExit()
    {
        PlayerEvents.OnBriefcaseDropped -= HandleBriefcaseDropped;
        _npc?.Agent.ResetPath();
    }

    public void OnTick()
    {
        switch (_phase)
        {
            case Phase.Watching: TickWatching(); break;
            case Phase.ChasingPlayer: TickChasing(); break;
            case Phase.SeekingBriefcase: TickSeekingBriefcase(); break;
            case Phase.RunningWithBriefcase: TickRunningWithBriefcase(); break;
        }
    }

    // ── Phases ───────────────────────────────────────────────────────────────

    private void TickWatching()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(_npc.transform.position, _playerTransform.position);
        if (dist <= _detectionRadius)
            EnterPhase(Phase.ChasingPlayer);
    }

    private void TickChasing()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(_npc.transform.position, _playerTransform.position);

        if (dist <= _pushDistance && Time.time - _lastPushTime >= _pushCooldown)
        {
            ExecutePush();
            return;
        }

        _npc.Agent.SetDestination(_playerTransform.position);
    }

    private void TickSeekingBriefcase()
    {
        if (_briefcaseController == null) return;

        // Tente de ramasser si assez proche
        if (_briefcaseController.TryPickup())
        {
            EnterPhase(Phase.RunningWithBriefcase);
            return;
        }

        // Se dirige vers la malette la plus proche
        BriefcaseItem nearest = FindNearestBriefcase();
        if (nearest != null)
            _npc.Agent.SetDestination(nearest.transform.position);
    }

    private void TickRunningWithBriefcase()
    {
        if (!_npc.Agent.pathPending && _npc.Agent.remainingDistance <= _waypointTolerance)
            PickRandomDestination();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void ExecutePush()
    {
        _lastPushTime = Time.time;

        // Oriente vers le joueur
        Vector3 dir = (_playerTransform.position - _npc.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            _npc.transform.rotation = Quaternion.LookRotation(dir.normalized);

        _npc.Agent.ResetPath();
        _npc.AnimController.TriggerPush();
        PlayerEvents.RaiseSweepFallStarted();
    }

    private void HandleBriefcaseDropped()
    {
        // Ne réagit que si on attend la malette
        if (_phase is not Phase.ChasingPlayer and not Phase.Watching) return;
        if (_briefcaseController == null || _briefcaseController.HasBriefcase) return;

        EnterPhase(Phase.SeekingBriefcase);
    }

    private void EnterPhase(Phase phase)
    {
        _phase = phase;

        switch (phase)
        {
            case Phase.Watching:
                _npc.Agent.isStopped = true;
                _npc.Agent.ResetPath();
                break;

            case Phase.ChasingPlayer:
            case Phase.SeekingBriefcase:
                _npc.Agent.isStopped = false;
                _npc.Agent.speed = _runSpeed;
                break;

            case Phase.RunningWithBriefcase:
                _npc.Agent.isStopped = false;
                _npc.Agent.speed = _runSpeed;
                PickRandomDestination();
                break;
        }
    }

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
