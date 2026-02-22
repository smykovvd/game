using UnityEngine;

public class BushVisibilityZone : MonoBehaviour
{
    public GameObject visibilityMask; // ссылка на объект маски (можно найти по тегу или назначить вручную)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            visibilityMask.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            visibilityMask.SetActive(false);
        }
    }
}