using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChoiceTrigger : MonoBehaviour
{
    [Header("Варианты выбора")]
    [SerializeField] private string[] options = new string[3];

    [Header("Сцены для загрузки")]
    [SerializeField] private SceneReference[] sceneReferences = new SceneReference[3];

    [Header("Настройки")]
    [SerializeField] private ChoiceManager choiceManager;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool startLocked = false; // Заблокирован ли триггер изначально

    private bool triggered = false;
    private bool isUnlocked;

    void Start()
    {
        if (startLocked)
            Lock();
        else
            Unlock();
    }

    // Разблокировать триггер (включить коллайдер и возможность срабатывания)
    public void Unlock()
    {
        isUnlocked = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        // Здесь можно добавить визуальный эффект (например, включить частицы)
    }

    // Заблокировать триггер (отключить коллайдер)
    public void Lock()
    {
        isUnlocked = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        // Отключить визуальный эффект
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked) return;
        if (oneTimeUse && triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (choiceManager != null)
            {
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