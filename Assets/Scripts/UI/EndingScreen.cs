using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Экран финала (сцена концовки). По сумме выборов из GameState показывает
/// одну из трёх концовок: текст + символ. Повесить на объект в сцене концовки.
/// </summary>
public class EndingScreen : MonoBehaviour
{
    [System.Serializable]
    public class EndingContent
    {
        [TextArea(3, 8)] public string text;
        public Sprite symbol;
    }

    [Header("Три концовки (индекс 0/1/2 = результат ResolveEnding)")]
    [SerializeField] private EndingContent[] endings = new EndingContent[3];

    [Header("UI")]
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private Image symbolImage;

    [Header("Возврат")]
    [SerializeField] private string menuSceneName = "MainMenu";

    void Start()
    {
        int index = GameState.Instance.ResolveEnding();
        if (endings == null || index < 0 || index >= endings.Length || endings[index] == null)
        {
            Debug.LogWarning($"EndingScreen: контент для концовки {index} не задан.");
            return;
        }

        var content = endings[index];
        if (endingText != null)
            endingText.text = content.text;

        if (symbolImage != null)
        {
            symbolImage.sprite = content.symbol;
            symbolImage.enabled = content.symbol != null;
        }

        Debug.Log($"EndingScreen: показана концовка {index}");
    }

    /// <summary>Повесить на кнопку «В меню».</summary>
    public void ReturnToMenu()
    {
        GameState.Instance.ResetRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
