using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Music")]
    [SerializeField] private AudioClip pauseMusic;
    [SerializeField] private float musicVolume = 0.5f;

    private bool isPaused = false;
    private GameObject currentDialogCanvas;
    private AudioSource audioSource;
    private AudioSource backgroundMusicSource;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;

        FindAndSaveBackgroundMusic();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void FindDialogInActiveScenes()
    {
        GameObject foundDialog = null;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "DialogCanvas")
                {
                    foundDialog = root;
                    break;
                }
            }
            if (foundDialog != null) break;
        }

        currentDialogCanvas = foundDialog;
    }

    private void FindAndSaveBackgroundMusic()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);

        foreach (AudioSource source in allAudioSources)
        {
            if (source != audioSource && source.playOnAwake && source.loop)
            {
                backgroundMusicSource = source;
                Debug.Log($"Найдена фоновая музыка: {source.gameObject.name}");
                break;
            }
        }

        if (backgroundMusicSource == null)
        {
            foreach (AudioSource source in allAudioSources)
            {
                if (source != audioSource)
                {
                    backgroundMusicSource = source;
                    Debug.Log($"Найдена музыка (запасной вариант): {source.gameObject.name}");
                    break;
                }
            }
        }
    }

    private void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
            Debug.Log("Фоновая музыка остановлена");
        }
    }

    private void ResumeBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.UnPause();
            Debug.Log("Фоновая музыка возобновлена");
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        FindDialogInActiveScenes();
        if (currentDialogCanvas != null)
            currentDialogCanvas.SetActive(false);

        StopBackgroundMusic();

        if (pauseMusic != null && !audioSource.isPlaying)
        {
            audioSource.clip = pauseMusic;
            audioSource.Play();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (currentDialogCanvas != null)
            currentDialogCanvas.SetActive(true);

        if (audioSource.isPlaying)
            audioSource.Stop();

        ResumeBackgroundMusic();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}