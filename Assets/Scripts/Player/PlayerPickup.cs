using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public Transform carryPoint;

    private Stone currentStone = null;
    private StonePileZone currentPileZone = null;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (currentStone != null)
            {
                TryPlaceStone();
            }
            else
            {
                TryPickUpStone();
            }
        }
    }

    void TryPickUpStone()
    {
        // 1. ѕытаемс€ вз€ть камень из кучи, если стоим в зоне и куча не пуста
        if (currentPileZone != null && currentPileZone.currentStones > 0)
        {
            if (currentPileZone.TryTakeStone(out Stone stoneFromPile))
            {
                currentStone = stoneFromPile;
                currentStone.PickUp(carryPoint);
                return;
            }
        }

        // 2. »наче ищем камень на полу (как раньше)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hit in hits)
        {
            Stone rock = hit.GetComponent<Stone>();
            if (rock != null && !rock.isCarried) // предполагаем, что у Stone есть флаг isCarried
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
            //  ладЄм камень в кучу
            currentPileZone.ReceiveStone(currentStone);
            currentStone = null;
        }
        else
        {
            // Ѕросаем камень на пол
            Vector3 dropPos = transform.position + transform.forward * 0.5f;
            currentStone.Drop(dropPos);
            currentStone = null;
        }
    }

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