using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibilityTime = 1f;

    private int currentHealth;
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Здоровье игрока: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"Игрок получил {damage} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Простая неуязвимость без эффектов
            isInvincible = true;
            Invoke(nameof(ResetInvincibility), invincibilityTime);
        }
    }

    void ResetInvincibility()
    {
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Игрок умер!");
        // gameObject.SetActive(false); // Отключить игрока
    }
}