using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Affiche l'écran de fin de jeu avec le message approprié selon si le joueur a la malette ou non.
/// </summary>
public class EndGameScreenController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [Header("Panneau")]
    [SerializeField] private GameObject _endScreen;

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _endMessageText;
    [SerializeField][TextArea] private string _goodEndingMessage = "Félicitations !\nVous avez réussi à garder la mallette. Bravo !";
    [SerializeField][TextArea] private string _badEndingMessage = "Vous avez terminé...\nmais la mallette a été perdue en chemin.";

    [Header("Boutons")]
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _endScreen.SetActive(false);
        _menuButton.onClick.AddListener(OnMenuClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>Active l'écran de fin et affiche le message selon l'état de la malette.</summary>
    public void ShowEndScreen(bool hasBriefcase)
    {
        _endScreen.SetActive(true);
        _endMessageText.text = hasBriefcase ? _goodEndingMessage : _badEndingMessage;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
