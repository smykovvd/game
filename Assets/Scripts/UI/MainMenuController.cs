using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] string firstLevelSceneName = "лабиринт";

    public void Play()
    {
        if (!string.IsNullOrEmpty(firstLevelSceneName))
            SceneManager.LoadScene(firstLevelSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
