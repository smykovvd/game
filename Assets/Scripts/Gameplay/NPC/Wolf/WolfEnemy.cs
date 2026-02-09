using UnityEngine;

public class WolfEnemy : MonoBehaviour
{
    public float detectionRange = 5f;
    public float chaseRange = 10f;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;
    public int damage = 2;
    public float attackCooldown = 1f;

    private Transform player;
    private Vector2 startPosition;
    private bool isChasing = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);

        if (isChasing)
        {
            // Преследование игрока
            if (distanceToPlayer > 0.5f && distanceFromStart <= chaseRange)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    chaseSpeed * Time.deltaTime
                );
            }

            // Проверяем, не ушел ли игрок слишком далеко
            if (distanceFromStart > chaseRange)
            {
                StopChasing();
            }
        }
        else
        {
            // Проверка обнаружения игрока
            if (distanceToPlayer <= detectionRange)
            {
                StartChasing();
            }

            // Возврат на стартовую позицию
            if (Vector2.Distance(transform.position, startPosition) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    startPosition,
                    returnSpeed * Time.deltaTime
                );
            }
        }
    }

    void StartChasing()
    {
        isChasing = true;
        Debug.Log("Волк начал преследование!");
    }

    void StopChasing()
    {
        isChasing = false;
        Debug.Log("Волк прекратил преследование!");
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
        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Зона преследования
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPosition, chaseRange);
    }
}