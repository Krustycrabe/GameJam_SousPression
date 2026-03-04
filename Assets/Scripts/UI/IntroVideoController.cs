using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Joue la vidéo d'introduction en plein écran.
/// Charge le niveau principal à la fin, ou immédiatement si le joueur appuie sur Skip.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class IntroVideoController : MonoBehaviour
{
    private const string GameSceneName = "LevelPrincipal";

    [Header("Références")]
    [SerializeField] private RawImage _videoDisplay;
    [SerializeField] private Button _skipButton;

    [Header("Vidéo")]
    [SerializeField] private VideoClip _introClip;

    private VideoPlayer _videoPlayer;
    private bool _isLoading;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        ConfigureVideoPlayer();
    }

    private void Start()
    {
        _skipButton.onClick.AddListener(LoadGame);
        _videoPlayer.Play();
    }

    private void ConfigureVideoPlayer()
    {
        _videoPlayer.clip = _introClip;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.isLooping = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _videoPlayer.loopPointReached += OnVideoFinished;

        // Crée une RenderTexture aux dimensions de l'écran et l'assigne
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        _videoPlayer.targetTexture = renderTexture;
        _videoDisplay.texture = renderTexture;
    }

    private void OnVideoFinished(VideoPlayer vp) => LoadGame();

    private void LoadGame()
    {
        if (_isLoading) return;
        _isLoading = true;

        _videoPlayer.Stop();
        SceneManager.LoadScene(GameSceneName);
    }

    private void OnDestroy()
    {
        _videoPlayer.loopPointReached -= OnVideoFinished;

        // Libère la RenderTexture
        if (_videoPlayer.targetTexture != null)
            _videoPlayer.targetTexture.Release();
    }
}
