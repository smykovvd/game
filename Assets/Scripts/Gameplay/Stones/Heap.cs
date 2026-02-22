using UnityEngine;
using UnityEngine.Events;

public class StonePileZone : MonoBehaviour
{
    public int requiredStones = 3;       // Сколько камней нужно собрать в этой куче
    public int currentStones = 0;         // Сколько уже положили
    public UnityEvent onPileCompleted;   // Событие, когда куча собрана (можно активировать дверь, триггер и т.п.)

    // Вызывается игроком, когда он кладёт камень в эту зону
    public void ReceiveStone(Stone rock)
    {
        if (currentStones >= requiredStones)
        {
            // Куча уже полна – не принимаем больше камней (можно вернуть игроку или просто проигнорировать)
            Debug.Log("Эта куча уже полна!");
            return;
        }

        // Помещаем камень в зону (можно положить в конкретную точку или просто уничтожить)
        // Для простоты – уничтожим объект камня (или сделаем его неактивным)
        // Но если нужно, чтобы камни оставались, можно переместить их в позицию внутри зоны.
        Vector3 pilePosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        rock.Drop(pilePosition);

        // Отключаем возможность снова поднять этот камень (например, меняем слой или убираем коллайдер)
        // Проще всего уничтожить компонент Stone, чтобы он больше не реагировал на поднятие:
        Destroy(rock.GetComponent<Stone>());
        // Или можно просто отключить коллайдер камня и сделать его статичным.

        currentStones++;
        Debug.Log($"Камень положен в кучу. Теперь {currentStones}/{requiredStones}");

        if (currentStones == requiredStones)
        {
            onPileCompleted.Invoke();
        }
    }
}