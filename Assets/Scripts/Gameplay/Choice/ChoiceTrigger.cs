using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChoiceTrigger : MonoBehaviour
{
    [Header("Тексты вариантов")]
    [SerializeField] private string[] options = new string[3];

    [Header("Сцены для вариантов")]
    [SerializeField] private SceneReference[] sceneReferences = new SceneReference[3];

    [Header("Подсказка Файрена (текст в окне выбора)")]
    [TextArea(2, 4)]
    [SerializeField] private string fairenHint = "";

    [Header("Влияние на концовку")]
    [Tooltip("Уникальный id развилки, напр. Scene2_Rasputye")]
    [SerializeField] private string choiceKey = "";
    [Tooltip("За какую концовку (0/1/2) голосует каждый вариант. -1 = не влияет.")]
    [SerializeField] private int[] endingVotes = new int[3] { -1, -1, -1 };

    [Header("Менеджер")]
    [SerializeField] private ChoiceManager choiceManager;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool startLocked = false; // ������������ �� ������� ����������

    private bool triggered = false;
    private bool isUnlocked;

    void Start()
    {
        if (startLocked)
            Lock();
        else
            Unlock();
    }

    // �������������� ������� (�������� ��������� � ����������� ������������)
    public void Unlock()
    {
        isUnlocked = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        // ����� ����� �������� ���������� ������ (��������, �������� �������)
    }

    // ������������� ������� (��������� ���������)
    public void Lock()
    {
        isUnlocked = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        // ��������� ���������� ������
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

                choiceManager.ShowChoicePanel(options, sceneNames, fairenHint, choiceKey, endingVotes);
            }
            else
            {
                Debug.LogWarning("ChoiceManager не назначен!");
            }
        }
    }

    // ��� �������: ����� ��������
    public void ResetTrigger()
    {
        triggered = false;
    }
}

// ����� ��� ���������� ������ �� �����
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
        // ������������� ��������� ��� ����� ��� ��������� SceneAsset
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif
}