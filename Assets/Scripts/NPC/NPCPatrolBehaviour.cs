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

        if (!_npc.Agent.enabled) return;
        _npc.Agent.isStopped = false;

        // Va au waypoint le plus proche — évite les demi-tours brusques au retour
        _currentWaypointIndex = FindClosestWaypointIndex();
        MoveToCurrentWaypoint();
    }

    public void OnExit()
    {
        if (_npc?.Agent != null && _npc.Agent.enabled)
            _npc.Agent.ResetPath();
    }

    public void OnTick()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        if (!_npc.Agent.enabled || !_npc.Agent.isOnNavMesh) return;
        if (_npc.Agent.pathPending) return;
        if (_npc.Agent.remainingDistance > _waypointTolerance) return;

        AdvanceAndMove();
    }

    private void MoveToCurrentWaypoint()
    {
        if (_waypoints.Length == 0) return;
        _npc.Agent.SetDestination(_waypoints[_currentWaypointIndex].position);
    }

    private void AdvanceAndMove()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        MoveToCurrentWaypoint();
    }

    /// <summary>Retourne l'index du waypoint le plus proche de la position actuelle.</summary>
    private int FindClosestWaypointIndex()
    {
        if (_waypoints == null || _waypoints.Length == 0) return 0;

        int closest = 0;
        float closestDist = float.MaxValue;

        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (_waypoints[i] == null) continue;
            float dist = Vector3.Distance(_npc.transform.position, _waypoints[i].position);
            if (dist < closestDist) { closestDist = dist; closest = i; }
        }

        return closest;
    }
}
