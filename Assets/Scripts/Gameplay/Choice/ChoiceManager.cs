using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private Button closeButton; 

    private List<string> sceneNames = new List<string>();

    void Start()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        
        if (choicePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    public void ShowChoicePanel(string[] options, string[] scenes)
    {
        if (options == null || scenes == null || options.Length != scenes.Length)
        {
            Debug.LogError("Массивы options и scenes должны быть одинаковой длины!");
            return;
        }

        ClearButtons();
        sceneNames = new List<string>(scenes);

        for (int i = 0; i < options.Length; i++)
        {
            
            if (!string.IsNullOrEmpty(options[i]) && !string.IsNullOrEmpty(scenes[i]))
            {
                CreateChoiceButton(i, options[i]);
                Debug.Log($"создана кнопка{i + 1}");
            }
        }

        choicePanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Панель открыта");
    }

    private void CreateChoiceButton(int index, string optionText)
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonsContainer);
        buttonObj.name = "ChoiceButton_" + index;

        TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = (index + 1) + ". " + optionText;
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int choiceIndex = index;
            button.onClick.AddListener(() => OnChoiceMade(choiceIndex));
        }
    }

    private void OnChoiceMade(int choiceIndex)
    {
        Debug.Log($"Выбран вариант {choiceIndex + 1}");
        LoadScene(choiceIndex);
    }

    private void LoadScene(int choiceIndex)
    {
        Time.timeScale = 1f;
        choicePanel.SetActive(false);

        if (choiceIndex < sceneNames.Count && !string.IsNullOrEmpty(sceneNames[choiceIndex]))
        {
            string sceneToLoad = sceneNames[choiceIndex];

            if (IsSceneInBuildSettings(sceneToLoad))
            {
                ClearButtons();
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError($"Сцена '{sceneToLoad}' не найдена в Build Settings!");
                ClearButtons();
            }
        }
        else
        {
            Debug.LogError($"Сцена для выбора {choiceIndex} не назначена!");
            ClearButtons();
        }
    }

    // Проверка наличия сцены в Build Settings
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild == sceneName)
                return true;
        }
        return false;
    }

    private void ClearButtons()
    {
        if (buttonsContainer == null) return;

        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }
    }


    public void ClosePanel()
    {
        Time.timeScale = 1f;
        choicePanel.SetActive(false);
        ClearButtons();
        Debug.Log("Панель закрыта");
    }
}