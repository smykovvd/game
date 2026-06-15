using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSystemLoader : MonoBehaviour
{
    void Start()
    {
        if (!IsSceneLoaded("PauseSystem"))
        {
            SceneManager.LoadScene("PauseSystem", LoadSceneMode.Additive);
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName && scene.isLoaded)
                return true;
        }
        return false;
    }
}