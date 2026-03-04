using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Gère les boutons du menu principal.</summary>
public class MainMenuController : MonoBehaviour
{
    private const string GameSceneName = "LevelPrincipal";

    [Header("Boutons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked() => SceneManager.LoadScene(GameSceneName);

    private void OnQuitClicked() => Application.Quit();
}
