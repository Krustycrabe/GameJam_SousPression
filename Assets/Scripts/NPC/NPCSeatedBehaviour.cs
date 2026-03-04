using UnityEngine;

/// <summary>Le NPC reste assis — agent et collider désactivés pour éviter toute interaction physique.</summary>
public class NPCSeatedBehaviour : MonoBehaviour, INPCBehaviour
{
    [Tooltip("Transform de la chaise — positionne le NPC précisément au démarrage.")]
    [SerializeField] private Transform _seatTransform;

    private NPCController _npc;
    private CapsuleCollider _capsuleCollider;

    public void OnEnter(NPCController npc)
    {
        _npc = npc;
        _capsuleCollider = npc.GetComponent<CapsuleCollider>();

        if (_seatTransform != null)
            npc.transform.SetPositionAndRotation(_seatTransform.position, _seatTransform.rotation);

        // Désactive agent ET collider — aucune interaction physique possible
        _npc.Agent.enabled = false;
        if (_capsuleCollider) _capsuleCollider.enabled = false;

        _npc.AnimController.SetSpeed(0f);
        _npc.AnimController.SetSeated(true);
    }

    public void OnExit()
    {
        if (_npc == null) return;
        if (_capsuleCollider) _capsuleCollider.enabled = true;
        _npc.Agent.enabled = true;
        _npc.AnimController.SetSeated(false);
    }

    public void OnTick() { }
}
