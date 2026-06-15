using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public AudioClip slashSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlaySlashSound()
    {
        if (slashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slashSound);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем врага по тегу или компоненту
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"Атаковал врага! Нанес {damage} урона");
        }
    }
}