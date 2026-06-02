using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Линейный переход между сценами: когда игрок входит в зону-триггер,
/// загружается заданная сцена. Используется для прямых связей по диаграмме
/// (Сцена 1 → Сцена 2, Сцена 6 → Сцена 7 и т.д.).
///
/// Повесить на пустой объект с Collider2D (Is Trigger = on).
/// Использует SceneReference (определён в ChoiceTrigger.cs) — целевую сцену
/// можно перетащить прямо ассетом в инспекторе.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneExitTrigger : MonoBehaviour
{
    [Header("Куда ведёт переход")]
    [SerializeField] private SceneReference targetScene;

    [Header("Условие прохода (необязательно)")]
    [Tooltip("Если задано — пройти можно только при наличии этого предмета в GameState.")]
    [SerializeField] private string requiredItemId = "";

    [Tooltip("Реплика, если предмета не хватает (показывается через подключённый FairenDialog).")]
    [SerializeField] private string lockedMessage = "Ещё рано идти дальше...";
    [SerializeField] private FairenDialog blockedHint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(requiredItemId) && !GameState.Instance.HasItem(requiredItemId))
        {
            if (blockedHint != null)
                blockedHint.PlaySingle(lockedMessage);
            else
                Debug.Log($"SceneExitTrigger: нужен предмет '{requiredItemId}', проход закрыт.");
            return;
        }

        string scene = targetScene != null ? targetScene.SceneName : null;
        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogError("SceneExitTrigger: целевая сцена не задана!", this);
            return;
        }

        GameState.Instance.SetLastScene(scene);
        // На случай, если ввод был заблокирован диалогом — снимаем блок перед загрузкой.
        GameState.InputBlocked = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }
}
