using UnityEngine;
using UnityEngine.Events;

public class StonePuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public struct PileRequirement
    {
        public StonePileZone pile;      // ссылка на кучу
        public int requiredCount;       // сколько камней должно быть в этой куче
    }

    public PileRequirement[] requirements;      // список куч с требуемыми значениями
    public UnityEvent onPuzzleSolved;           // событие при решении головоломки
    public UnityEvent onPuzzleUnsolved;         // событие, если решение перестало выполняться

    private bool puzzleSolved = false;

    void Start()
    {
        // Подписываемся на изменения каждой кучи
        foreach (var req in requirements)
        {
            if (req.pile != null)
            {
                req.pile.onStonesChanged.AddListener(CheckPuzzle);
            }
            else
            {
                Debug.LogWarning("Pile requirement has null pile reference", this);
            }
        }

        // Первоначальная проверка
        CheckPuzzle();
    }

    void CheckPuzzle()
    {
        bool solved = true;
        foreach (var req in requirements)
        {
            if (req.pile == null) continue;
            if (req.pile.currentStones != req.requiredCount)
            {
                solved = false;
                break;
            }
        }

        if (solved && !puzzleSolved)
        {
            puzzleSolved = true;
            onPuzzleSolved.Invoke();
        }
        else if (!solved && puzzleSolved)
        {
            puzzleSolved = false;
            onPuzzleUnsolved?.Invoke();
        }
    }
}