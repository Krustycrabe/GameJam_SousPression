using UnityEngine;

/// <summary>Le NPC passe le balai sur place.</summary>
public class NPCSweepBehaviour : MonoBehaviour, INPCBehaviour
{
    private NPCController _npc;

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _npc.Agent.isStopped = true;
        _npc.Agent.velocity = Vector3.zero;
        _npc.AnimController.SetSweeping(true);
    }

    public void OnExit() => _npc?.AnimController.SetSweeping(false);
    public void OnTick() { }
}
