using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class StonePileZone : MonoBehaviour
{
    public int capacity = 3;
    public int currentStones = 0;
    public UnityEvent onPileCompleted;
    public UnityEvent onPileEmptied;
    public UnityEvent onStonesChanged; // новое событие

    private List<Stone> stonesInPile = new List<Stone>();

    public void ReceiveStone(Stone rock)
    {
        if (currentStones >= capacity)
        {
            Debug.Log("Куча уже полна!");
            return;
        }

        Vector3 pilePosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        rock.Drop(pilePosition);

        Collider2D col = rock.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        stonesInPile.Add(rock);
        currentStones++;

        // Вызываем события
        onStonesChanged?.Invoke();
        if (currentStones == capacity)
            onPileCompleted.Invoke();
    }

    public bool TryTakeStone(out Stone takenStone)
    {
        takenStone = null;
        if (currentStones <= 0)
        {
            Debug.Log("Куча пуста");
            return false;
        }

        takenStone = stonesInPile[stonesInPile.Count - 1];
        stonesInPile.RemoveAt(stonesInPile.Count - 1);
        currentStones--;

        Collider2D col = takenStone.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // Вызываем события
        onStonesChanged?.Invoke();
        if (currentStones == 0)
            onPileEmptied?.Invoke();

        return true;
    }
}