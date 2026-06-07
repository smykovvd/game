using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Сюжетный предмет (например, «Осколок судьбы»). Не часть инвентарной механики:
/// при подборе просто отмечается в GameState и может запустить реплику Файрена.
///
/// Повесить на объект со спрайтом и Collider2D (Is Trigger = on).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StoryItemPickup : MonoBehaviour
{
    [Header("Предмет")]
    [SerializeField] private string itemId = "shard_of_fate";
    [SerializeField] private string displayName = "Осколок судьбы";

    [Tooltip("За какую концовку (0/1/2) голосует подбор предмета. -1 = не влияет.")]
    [SerializeField] private int endingVote = -1;

    [Header("Как подбирается")]
    [Tooltip("true — подбор сразу при касании; false — по клавише взаимодействия.")]
    [SerializeField] private bool pickOnTouch = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Реакция при подборе (необязательно)")]
    [SerializeField] private FairenDialog pickupDialog;
    [TextArea(2, 4)]
    [SerializeField] private string pickupLine = "";
    public UnityEvent onPicked;

    bool playerInRange;
    bool picked;

    void Update()
    {
        if (pickOnTouch || picked || !playerInRange) return;

        if (Input.GetKeyDown(interactKey))
            Pick();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (pickOnTouch)
            Pick();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Pick()
    {
        if (picked) return;
        picked = true;

        GameState.Instance.AddItem(itemId);
        if (endingVote >= 0)
            GameState.Instance.RecordChoice($"item_{itemId}", 0, endingVote);
        Debug.Log($"Подобран предмет: {displayName} ({itemId})");

        if (pickupDialog != null && !string.IsNullOrEmpty(pickupLine))
            pickupDialog.PlaySingle(pickupLine);

        onPicked?.Invoke();

        // Предмет исчезает с карты, но остаётся отмеченным в GameState.
        gameObject.SetActive(false);
    }
}
