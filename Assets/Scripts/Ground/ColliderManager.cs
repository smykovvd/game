using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoEdgeCollider : MonoBehaviour
{
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private bool createAtStart = true;

    private void Start()
    {
        if (createAtStart)
            UpdateEdgeCollider();
    }

    [ContextMenu("Update Edge Collider")]
    public void UpdateEdgeCollider()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Bounds bounds = spriteRenderer.bounds;

        Vector2[] points = new Vector2[5];
        points[0] = transform.InverseTransformPoint(new Vector3(
            bounds.min.x + offset,
            bounds.min.y + offset,
            0));
        points[1] = transform.InverseTransformPoint(new Vector3(
            bounds.max.x - offset,
            bounds.min.y + offset,
            0));
        points[2] = transform.InverseTransformPoint(new Vector3(
            bounds.max.x - offset,
            bounds.max.y - offset,
            0));
        points[3] = transform.InverseTransformPoint(new Vector3(
            bounds.min.x + offset,
            bounds.max.y - offset,
            0));
        points[4] = points[0];

        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();

        edgeCollider.points = points;
    }
}