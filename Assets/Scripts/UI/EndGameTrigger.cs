using UnityEngine;

/// <summary>
/// BoxCollider trigger à placer à la fin du niveau.
/// Déclenche l'écran de fin en vérifiant si le joueur a la malette.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EndGameTrigger : MonoBehaviour
{
    [SerializeField] private EndGameScreenController _endGameScreen;

    private bool _triggered;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;

        PlayerBriefCaseController briefcase = other.GetComponent<PlayerBriefCaseController>();
        bool hasBriefcase = briefcase != null && briefcase.IsHolding;

        _endGameScreen.ShowEndScreen(hasBriefcase);
    }
}
