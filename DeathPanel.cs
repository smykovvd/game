using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text deathText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Настройки")]
    [SerializeField] private string deathMessage = "Ты погиб...";
    [SerializeField] private bool blockInput = true;
    [SerializeField] private float showDelay = 1f;

    private PlayerHealth playerHealth;
    private bool isDead = false;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += CheckDeath;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMainMenu);
    }

    void CheckDeath(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Invoke(nameof(ShowPanel), showDelay);
        }
    }

    void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);

            if (deathText != null)
                deathText.text = deathMessage;

            if (blockInput)
                GameState.InputBlocked = true;
        }
    }

    void RestartGame()
    {
        if (blockInput)
            GameState.InputBlocked = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToMainMenu()
    {
        if (blockInput)
            GameState.InputBlocked = false;

        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= CheckDeath;
        }

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.RemoveListener(GoToMainMenu);
    }
}
