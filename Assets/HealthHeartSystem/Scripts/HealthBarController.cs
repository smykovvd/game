/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 *  Adapted for PlayerHealth by Natalia
 */

using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;

    public Transform heartsParent;
    public GameObject heartContainerPrefab;

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("HealthBarController: PlayerHealth не найден!");
            return;
        }

        // Создаём массивы по максимальному здоровью (3 сердечка)
        heartContainers = new GameObject[playerHealth.maxHealth];
        heartFills = new Image[playerHealth.maxHealth];

        // Подписываемся на событие изменения здоровья
        playerHealth.OnHealthChanged += UpdateHeartsHUD;

        // Создаём сердечки
        InstantiateHeartContainers();

        // Обновляем отображение
        UpdateHeartsHUD(playerHealth.currentHealth, playerHealth.maxHealth);
    }

    public void UpdateHeartsHUD(int currentHealth, int maxHealth)
    {
        SetFilledHearts(currentHealth);
    }

    void SetFilledHearts(int currentHealth)
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < currentHealth)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < playerHealth.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHeartsHUD;
        }
    }
}