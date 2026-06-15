using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibilityTime = 1f;
    public float blinkInterval = 0.1f;

    public int currentHealth { get; private set; }
    private bool isInvincible = false;
    private bool isDead = false;
    private SpriteRenderer[] spriteRenderers;

    public event Action<int, int> OnHealthChanged;
    private CharacterAnimationBridge animationBridge;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        animationBridge = GetComponent<CharacterAnimationBridge>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        Debug.Log($"Здоровье игрока: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0 || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            StartCoroutine(BlinkEffect());
            Invoke(nameof(ResetInvincibility), invincibilityTime);
        }
    }

    void ResetInvincibility()
    {
        isInvincible = false;
        SetSpritesVisible(true);
    }

    IEnumerator BlinkEffect()
    {
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityTime)
        {
            visible = !visible;
            SetSpritesVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetSpritesVisible(true);
    }

    void SetSpritesVisible(bool visible)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = visible ? 1f : 0.3f;
                sr.color = color;
            }
        }
    }

    void Die()
    {
        isDead = true;

        if (animationBridge != null)
            animationBridge.PlayDeath();

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Debug.Log("Игрок умер!");
    }
}