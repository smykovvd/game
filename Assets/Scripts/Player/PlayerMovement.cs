using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public GameObject attackHitbox;
    public float attackCooldown = 0.5f;
    public float attackDuration = 0.2f;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    void Update()
    {
        // Движение
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontal, vertical).normalized;

        // Сохраняем последнее направление для атаки
        if (movement != Vector2.zero)
        {
            lastMoveDirection = movement;
        }

        // Атака по пробелу (можно изменить)
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking) // Не двигаемся во время атаки
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Активируем хитбокс
        if (attackHitbox != null)
        {
            // Позиционируем хитбокс
            float attackDistance = 0.8f;
            Vector2 hitboxPosition = (Vector2)transform.position + lastMoveDirection * attackDistance;
            attackHitbox.transform.position = hitboxPosition;

            attackHitbox.SetActive(true);
        }

        // Отключаем атаку через время
        Invoke(nameof(EndAttack), attackDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }
}