using UnityEngine;

public class Stone : MonoBehaviour
{
    [HideInInspector] public bool isCarried = false;

    [Tooltip("Физический коллайдер (не триггер) для столкновений")]
    public Collider2D physicsCollider;

    private int bushesInside = 0;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Если физический коллайдер не назначен, пробуем найти любой коллайдер (не триггер)
        if (physicsCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    physicsCollider = col;
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bush"))
        {
            bushesInside++;
            if (!isCarried)
                UpdateMask();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bush"))
        {
            bushesInside--;
            if (!isCarried)
                UpdateMask();
        }
    }

    void UpdateMask()
    {
        if (spriteRenderer == null) return;

        if (isCarried)
        {
            // Поднятый камень всегда полностью видим
            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        }
        else
        {
            spriteRenderer.maskInteraction = (bushesInside > 0)
                ? SpriteMaskInteraction.VisibleInsideMask
                : SpriteMaskInteraction.None;
        }
    }

    public void PickUp(Transform carryPoint)
    {
        if (isCarried) return;
        isCarried = true;

        // Отключаем физический коллайдер, но триггерный остаётся активным
        if (physicsCollider != null)
            physicsCollider.enabled = false;
        if (rb != null)
            rb.simulated = false;

        UpdateMask();

        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop(Vector3 dropPosition)
    {
        if (!isCarried) return;
        isCarried = false;

        transform.SetParent(null);
        transform.position = dropPosition;

        if (physicsCollider != null)
            physicsCollider.enabled = true;
        if (rb != null)
            rb.simulated = true;

        UpdateMask();
    }
}