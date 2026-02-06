using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public float cameraHeight = -10f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(target.position.x, target.position.y);

        Vector2 smoothedPos = Vector2.Lerp(currentPos, targetPos, smoothSpeed);

        transform.position = new Vector3(smoothedPos.x, smoothedPos.y, cameraHeight);
    }
}