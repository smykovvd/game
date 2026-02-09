using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;

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