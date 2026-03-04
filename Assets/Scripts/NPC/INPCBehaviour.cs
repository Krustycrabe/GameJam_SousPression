/// <summary>Contrat commun de tous les comportements NPC.</summary>
public interface INPCBehaviour
{
    /// <summary>Appelé quand le comportement devient actif.</summary>
    void OnEnter(NPCController npc);

    /// <summary>Appelé quand le comportement est interrompu (hit, chase...).</summary>
    void OnExit();

    /// <summary>Appelé chaque frame tant que le comportement est actif.</summary>
    void OnTick();
}