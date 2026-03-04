using UnityEngine;

/// <summary>Le NPC parcourt une liste de waypoints en boucle.</summary>
public class NPCPatrolBehaviour : MonoBehaviour, INPCBehaviour
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _waypointTolerance = 0.5f;
    [SerializeField] private float _patrolSpeed = 3f;

    private NPCController _npc;
    private int _currentWaypointIndex;

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _npc.Agent.speed = _patrolSpeed;
        _npc.Agent.isStopped = false;
        MoveToNextWaypoint();
    }

    public void OnExit() => _npc?.Agent.ResetPath();

    public void OnTick()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        if (!_npc.Agent.pathPending && _npc.Agent.remainingDistance <= _waypointTolerance)
            MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (_waypoints.Length == 0) return;
        _npc.Agent.SetDestination(_waypoints[_currentWaypointIndex].position);
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
    }
}
