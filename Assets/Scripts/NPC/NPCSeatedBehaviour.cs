using UnityEngine;

/// <summary>Le NPC reste assis sans bouger.</summary>
public class NPCSeatedBehaviour : MonoBehaviour, INPCBehaviour
{
    [Tooltip("Transform de la chaise — positionne le NPC précisément au démarrage.")]
    [SerializeField] private Transform _seatTransform;

    public void OnEnter(NPCController npc)
    {
        npc.Agent.isStopped = true;
        npc.Agent.velocity = Vector3.zero;
        npc.AnimController.SetSpeed(0f);

        if (_seatTransform != null)
        {
            npc.transform.SetPositionAndRotation(
                _seatTransform.position,
                _seatTransform.rotation
            );
        }
    }

    public void OnExit() { }
    public void OnTick() { }
}
