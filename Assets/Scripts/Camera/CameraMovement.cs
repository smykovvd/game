using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public float cameraHeight = -10f;

    [Header("Границы (камера не выходит за пределы сцены)")]
    public bool useBounds = false;
    public Vector2 boundsMin;
    public Vector2 boundsMax;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(target.position.x, target.position.y);

        Vector2 smoothedPos = Vector2.Lerp(currentPos, targetPos, smoothSpeed);

        if (useBounds && cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float minX = boundsMin.x + halfW, maxX = boundsMax.x - halfW;
            float minY = boundsMin.y + halfH, maxY = boundsMax.y - halfH;

            // Если сцена уже обзора по оси — просто центрируем по этой оси.
            smoothedPos.x = minX <= maxX ? Mathf.Clamp(smoothedPos.x, minX, maxX) : (boundsMin.x + boundsMax.x) * 0.5f;
            smoothedPos.y = minY <= maxY ? Mathf.Clamp(smoothedPos.y, minY, maxY) : (boundsMin.y + boundsMax.y) * 0.5f;
        }

        transform.position = new Vector3(smoothedPos.x, smoothedPos.y, cameraHeight);
    }
}
