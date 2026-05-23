using TMPro;
using UnityEngine;

/// <summary>
/// World Space подсказка над персонажем. Привязать к DialogAnchor (дочерний Transform).
/// </summary>
public class WorldDialogBubble : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 worldOffset = new(0f, 1.2f, 0f);
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] GameObject bubbleRoot;
    [SerializeField] float defaultDuration = 3f;

    float hideTime = -1f;

    void Awake()
    {
        if (bubbleRoot == null)
            bubbleRoot = gameObject;

        HideImmediate();
    }

    void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + worldOffset;

        if (hideTime > 0f && Time.time >= hideTime)
            HideImmediate();
    }

    public void Show(string text, Transform target, float duration = -1f)
    {
        followTarget = target;
        if (messageText != null)
            messageText.text = text;

        bubbleRoot.SetActive(true);
        hideTime = Time.time + (duration > 0f ? duration : defaultDuration);
    }

    public void HideImmediate()
    {
        hideTime = -1f;
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}
