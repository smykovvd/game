using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Экран выбора. Показывает до трёх вариантов (кнопки + подсказка Файрена),
/// выбор — кликом или клавишами 1/2/3. Выбор записывается в GameState
/// (для определения концовки), затем грузится соответствующая сцена.
/// </summary>
public class ChoiceManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private Button closeButton;

    [Header("Подсказка Файрена")]
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Поведение")]
    [Tooltip("Разрешить закрыть панель без выбора (Escape/кнопка). Для обязательных развилок — выключить.")]
    [SerializeField] private bool allowClose = false;

    private List<string> sceneNames = new List<string>();
    private List<string> optionTexts = new List<string>();
    private string currentChoiceKey;
    private int[] currentEndingVotes;

    void Start()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(allowClose);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void Update()
    {
        if (choicePanel == null || !choicePanel.activeSelf) return;

        // Выбор клавишами 1/2/3.
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) TryChooseByKey(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) TryChooseByKey(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) TryChooseByKey(2);

        if (allowClose && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    void TryChooseByKey(int index)
    {
        if (index >= 0 && index < sceneNames.Count
            && !string.IsNullOrEmpty(optionTexts[index])
            && !string.IsNullOrEmpty(sceneNames[index]))
        {
            OnChoiceMade(index);
        }
    }

    /// <summary>Показать экран выбора. Вызывается из ChoiceTrigger.</summary>
    public void ShowChoicePanel(string[] options, string[] scenes, string hint = "",
                                string choiceKey = "", int[] endingVotes = null)
    {
        if (options == null || scenes == null || options.Length != scenes.Length)
        {
            Debug.LogError("ChoiceManager: массивы options и scenes должны быть одной длины!");
            return;
        }

        ClearButtons();
        sceneNames = new List<string>(scenes);
        optionTexts = new List<string>(options);
        currentChoiceKey = choiceKey;
        currentEndingVotes = endingVotes;

        if (hintText != null)
            hintText.text = hint;

        for (int i = 0; i < options.Length; i++)
        {
            if (!string.IsNullOrEmpty(options[i]) && !string.IsNullOrEmpty(scenes[i]))
                CreateChoiceButton(i, options[i]);
        }

        choicePanel.SetActive(true);
        Time.timeScale = 0f;
        GameState.InputBlocked = true;
    }

    private void CreateChoiceButton(int index, string optionText)
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonsContainer);
        buttonObj.name = "ChoiceButton_" + index;

        TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = (index + 1) + ". " + optionText;

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int choiceIndex = index;
            button.onClick.AddListener(() => OnChoiceMade(choiceIndex));
        }
    }

    private void OnChoiceMade(int choiceIndex)
    {
        int vote = (currentEndingVotes != null && choiceIndex < currentEndingVotes.Length)
            ? currentEndingVotes[choiceIndex]
            : -1;
        GameState.Instance.RecordChoice(currentChoiceKey, choiceIndex, vote);

        LoadScene(choiceIndex);
    }

    private void LoadScene(int choiceIndex)
    {
        Time.timeScale = 1f;
        GameState.InputBlocked = false;
        choicePanel.SetActive(false);

        if (choiceIndex < sceneNames.Count && !string.IsNullOrEmpty(sceneNames[choiceIndex]))
        {
            string sceneToLoad = sceneNames[choiceIndex];

            if (IsSceneInBuildSettings(sceneToLoad))
            {
                GameState.Instance.SetLastScene(sceneToLoad);
                ClearButtons();
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError($"Сцена '{sceneToLoad}' не добавлена в Build Settings!");
                ClearButtons();
            }
        }
        else
        {
            Debug.LogError($"Сцена для выбора {choiceIndex} не задана!");
            ClearButtons();
        }
    }

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
            Destroy(child.gameObject);
    }

    public void ClosePanel()
    {
        if (!allowClose) return;
        Time.timeScale = 1f;
        GameState.InputBlocked = false;
        choicePanel.SetActive(false);
        ClearButtons();
    }
}
