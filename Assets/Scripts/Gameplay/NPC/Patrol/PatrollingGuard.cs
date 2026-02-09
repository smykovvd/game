using UnityEngine;
using System.Collections.Generic;

public class PatrollingGuard : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public float moveSpeed = 2f;
    public int damage = 1;
    public float attackCooldown = 1f;

    private int currentPointIndex = 0;
    private float lastAttackTime = 0f;

    void Update()
    {
        if (patrolPoints.Count == 0) return;

        // Движение к текущей точке
        Transform targetPoint = patrolPoints[currentPointIndex];
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        // Если достигли точки, идем к следующей
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && CanAttack())
        {
            AttackPlayer(collision.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Непрерывный урон при контакте
        if (collision.gameObject.CompareTag("Player") && CanAttack())
        {
            AttackPlayer(collision.gameObject);
        }
    }

    bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    void AttackPlayer(GameObject player)
    {
        lastAttackTime = Time.time;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null) continue;

                Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);

                int nextIndex = (i + 1) % patrolPoints.Count;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }
    }
}