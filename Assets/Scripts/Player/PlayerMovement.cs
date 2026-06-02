using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public GameObject attackHitbox;
    public float attackCooldown = 0.5f;
    public float attackDuration = 0.2f;

    [SerializeField] CharacterAnimationBridge animationBridge;

    Vector2 movement;
    Vector2 lastMoveDirection = Vector2.down;
    float lastAttackTime;
    bool isAttacking;

    void Awake()
    {
        if (animationBridge == null)
            animationBridge = GetComponent<CharacterAnimationBridge>();
    }

    void Update()
    {
        // Во время диалогов/выбора ввод игрока заблокирован (пробел уходит на пропуск реплики).
        if (GameState.InputBlocked)
        {
            movement = Vector2.zero;
            animationBridge?.UpdateLocomotion(movement, isAttacking);
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontal, vertical).normalized;

        if (movement != Vector2.zero)
            lastMoveDirection = movement;

        animationBridge?.UpdateLocomotion(movement, isAttacking);

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
            Attack();
    }

    void FixedUpdate()
    {
        if (!isAttacking)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;

        animationBridge?.PlayAttack();

        if (attackHitbox != null)
        {
            float attackDistance = 0.8f;
            Vector2 hitboxPosition = (Vector2)transform.position + lastMoveDirection * attackDistance;
            attackHitbox.transform.position = hitboxPosition;
            attackHitbox.SetActive(true);
        }

        Invoke(nameof(EndAttack), attackDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        animationBridge?.EndAttackAnimation();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }
}
