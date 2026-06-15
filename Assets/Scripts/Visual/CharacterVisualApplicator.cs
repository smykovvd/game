using UnityEngine;

/// <summary>
/// Применяет CharacterVisualData к дочернему Visual (или к себе).
/// Художник меняет только .asset, не префаб.
/// </summary>
[DisallowMultipleComponent]
public class CharacterVisualApplicator : MonoBehaviour
{
    [SerializeField] CharacterVisualData visualData;
    [SerializeField] Transform visualRoot;
    [SerializeField] bool applyOnAwake = false;

    SpriteRenderer[] spriteRenderers;
    Animator animator;

    void Awake()
    {
        ResolveVisualRoot();
        spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        animator = visualRoot.GetComponentInChildren<Animator>(true);

        if (applyOnAwake)
            Apply();
    }

    void ResolveVisualRoot()
    {
        if (visualRoot != null)
            return;

        var visual = transform.Find("Visual");
        visualRoot = visual != null ? visual : transform;
    }

    public void Apply()
    {
        if (visualData == null)
            return;

        ResolveVisualRoot();

        if (animator == null)
            animator = visualRoot.GetComponentInChildren<Animator>(true);

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);

        if (animator != null && visualData.animatorController != null)
            animator.runtimeAnimatorController = visualData.animatorController;

        foreach (var sr in spriteRenderers)
        {
            if (!IsBodySpriteRenderer(sr))
                continue;

            if (visualData.defaultSprite != null && sr.sprite == null)
                sr.sprite = visualData.defaultSprite;
        }

        foreach (var sr in spriteRenderers)
        {
            if (!IsBodySpriteRenderer(sr))
                continue;

            sr.color = visualData.tint;

            if (!string.IsNullOrEmpty(visualData.sortingLayerName))
                sr.sortingLayerName = visualData.sortingLayerName;

            sr.sortingOrder = visualData.sortingOrder;
        }

        if (visualRoot == transform)
        {
            var scale = visualRoot.localScale;
            scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * visualData.visualScale;
            scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * visualData.visualScale;
            scale.z = 1f;
            visualRoot.localScale = scale;
        }
        else
        {
            visualRoot.localScale = Vector3.one * visualData.visualScale;
        }

        if (animator != null)
            animator.Update(0f);
    }

    static bool IsBodySpriteRenderer(SpriteRenderer sr)
    {
        if (sr == null)
            return false;

        if (sr.GetComponent<SpriteMask>() != null)
            return false;

        return sr.gameObject.name != "VisibilityMask";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!applyOnAwake || visualData == null)
            return;

        ResolveVisualRoot();
        spriteRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        animator = visualRoot.GetComponentInChildren<Animator>(true);
        Apply();
    }
#endif
}
