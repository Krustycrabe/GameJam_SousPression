using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Le NPC se déplace aléatoirement dans un rayon autour de sa position d'origine.
/// </summary>
public class NPCRandomWalkBehaviour : MonoBehaviour, INPCBehaviour
{
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _wanderRadius = 8f;
    [SerializeField] private float _waitDuration = 1.5f;
    [SerializeField] private float _waypointTolerance = 0.5f;

    private NPCController _npc;
    private Vector3 _origin;
    private float _waitTimer;
    private bool _isWaiting;

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _npc.Agent.speed = _walkSpeed;
        _origin = npc.transform.position;
        _isWaiting = false;
        PickRandomDestination();
    }

    public void OnExit() => _npc?.Agent.ResetPath();

    public void OnTick()
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                PickRandomDestination();
            }
            return;
        }

        if (!_npc.Agent.pathPending && _npc.Agent.remainingDistance <= _waypointTolerance)
        {
            _isWaiting = true;
            _waitTimer = _waitDuration;
        }
    }

    private void PickRandomDestination()
    {
        Vector3 randomPoint = _origin + Random.insideUnitSphere * _wanderRadius;
        randomPoint.y = _origin.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _wanderRadius, NavMesh.AllAreas))
            _npc.Agent.SetDestination(hit.position);
    }
}
