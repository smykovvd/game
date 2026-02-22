using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;     // Кнопка взаимодействия
    public Transform carryPoint;                 // Пустой объект – точка, куда крепится камень (например, перед персонажем)

    private Stone currentStone = null;              // Какой камень сейчас несём
    private StonePileZone currentPileZone = null; // В какой зоне укладки находимся

    void Update()
    {
        // Если нажата кнопка взаимодействия
        if (Input.GetKeyDown(interactKey))
        {
            if (currentStone != null)
            {
                // Уже несём камень – пытаемся положить
                TryPlaceStone();
            }
            else
            {
                // Не несём – пытаемся поднять
                TryPickUpStone();
            }
        }
    }

    void TryPickUpStone()
    {
        // Здесь нужен способ найти ближайший камень, с которым можно взаимодействовать.
        // Проще всего использовать триггер на самом камне (см. дополнение после кода).
        // Для примера реализуем через проверку всех коллайдеров в небольшом радиусе:
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hit in hits)
        {
            Stone rock = hit.GetComponent<Stone>();
            if (rock != null && !rock.isCarried)
            {
                currentStone = rock;
                rock.PickUp(carryPoint);
                break;
            }
        }
    }

    void TryPlaceStone()
    {
        if (currentPileZone != null)
        {
            // Мы в зоне укладки – кладём камень в кучу
            currentPileZone.ReceiveStone(currentStone);
            currentStone = null;
        }
        else
        {
            // Просто бросаем камень на пол (в текущую позицию игрока с небольшим смещением)
            Vector3 dropPos = transform.position + transform.forward * 0.5f; // смещение вперёд
            currentStone.Drop(dropPos);
            currentStone = null;
        }
    }

    // Для определения входа/выхода из зоны укладки используем триггеры
    void OnTriggerEnter2D(Collider2D other)
    {
        StonePileZone zone = other.GetComponent<StonePileZone>();
        if (zone != null)
        {
            currentPileZone = zone;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        StonePileZone zone = other.GetComponent<StonePileZone>();
        if (zone != null && zone == currentPileZone)
        {
            currentPileZone = null;
        }
    }
}