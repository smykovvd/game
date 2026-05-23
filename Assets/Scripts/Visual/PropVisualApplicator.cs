using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class PropVisualApplicator : MonoBehaviour
{
    [SerializeField] PropData propData;

    void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (propData == null)
            return;

        var sr = GetComponent<SpriteRenderer>();
        if (propData.sprite != null)
            sr.sprite = propData.sprite;

        sr.sortingLayerName = propData.sortingLayerName;
        sr.sortingOrder = propData.sortingOrder;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = propData.colliderSize;
            box.isTrigger = propData.isTrigger;
        }
    }
}
