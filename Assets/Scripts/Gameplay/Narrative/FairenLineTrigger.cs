using UnityEngine;

/// <summary>
/// Зона, при входе в которую игрок слышит одну реплику Файрена (через подключённый
/// FairenDialog). Удобно вешать на встречу с врагами, появление эха и т.п.
/// Повесить на пустой объект с Collider2D (Is Trigger = on).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FairenLineTrigger : MonoBehaviour
{
    [SerializeField] private FairenDialog dialog;
    [TextArea(2, 4)]
    [SerializeField] private string line;
    [SerializeField] private bool oneShot = true;

    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && used) return;
        used = true;

        if (dialog != null && !string.IsNullOrEmpty(line))
            dialog.PlaySingle(line);
    }
}
