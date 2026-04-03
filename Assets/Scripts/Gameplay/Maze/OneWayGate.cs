using UnityEngine;

public class OneWayGate : MonoBehaviour
{
    public GameObject wall; // стена, которая появится (изначально выключена)

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Триггер {name} обнаружил объект: {other.name}, тег: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Попал игрок! Активируем стену.");
            if (wall != null)
            {
                wall.SetActive(true);
                Debug.Log($"Стена {wall.name} активирована.");
            }
            else
            {
                Debug.LogError("Стена не назначена в инспекторе!");
            }

            // Отключаем триггер, чтобы не срабатывал повторно
            gameObject.SetActive(false);
            Debug.Log("Триггер отключён.");
        }
    }
}