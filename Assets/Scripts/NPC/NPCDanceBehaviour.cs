using UnityEngine;

/// <summary>
/// Le NPC danse sur place.
/// Dans l'Animator : bool IsDancing + float DanceIndex (0.0 → 1.0) → blend tree.
/// </summary>
public class NPCDanceBehaviour : MonoBehaviour, INPCBehaviour
{
    private const int DanceCount = 6;

    [Tooltip("Index de la danse (0 à 5) — mappé automatiquement sur 0.0 à 1.0 dans le blend tree.")]
    [Range(0, DanceCount - 1)]
    [SerializeField] private int _danceIndex = 0;

    private NPCController _npc;

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _npc.Agent.isStopped = true;
        _npc.Agent.velocity = Vector3.zero;
        _npc.AnimController.SetDancing(true, _danceIndex, DanceCount);
    }

    public void OnExit() => _npc?.AnimController.SetDancing(false);
    public void OnTick() { }
}
