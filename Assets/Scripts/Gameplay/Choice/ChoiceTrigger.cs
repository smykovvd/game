using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChoiceTrigger : MonoBehaviour
{
    [Header("Варианты выбора")]
    [SerializeField] private string[] options = new string[3]; // Тексты вариантов

    [Header("Сцены для загрузки")]
    [SerializeField] private SceneReference[] sceneReferences = new SceneReference[3]; // Ссылки на сцены

    [Header("Настройки")]
    [SerializeField] private ChoiceManager choiceManager;
    [SerializeField] private bool oneTimeUse = true;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (oneTimeUse && triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (choiceManager != null)
            {
                // Преобразуем SceneReference в массив имен сцен
                string[] sceneNames = new string[sceneReferences.Length];
                for (int i = 0; i < sceneReferences.Length; i++)
                {
                    sceneNames[i] = sceneReferences[i]?.SceneName;
                }

                choiceManager.ShowChoicePanel(options, sceneNames);
            }
            else
            {
                Debug.LogWarning("ChoiceManager не назначен!");
            }
        }
    }

    // Для отладки: сброс триггера
    public void ResetTrigger()
    {
        triggered = false;
    }
}

// Класс для безопасной ссылки на сцену
[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    [SerializeField] private string sceneName = "";

    public string SceneName
    {
        get
        {
#if UNITY_EDITOR
            return sceneAsset != null ? sceneAsset.name : sceneName;
#else
            return sceneName;
#endif
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автоматически обновляем имя сцены при изменении SceneAsset
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif
}