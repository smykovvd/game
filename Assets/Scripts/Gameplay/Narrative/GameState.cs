using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Единое состояние прохождения, живущее между сценами (DontDestroyOnLoad).
/// Хранит сделанные выборы, собранные сюжетные предметы и "голоса" за концовки,
/// а также имя последней сцены для функции "Продолжить".
///
/// Доступ через GameState.Instance — если объекта в сцене нет, он создаётся
/// автоматически (удобно при тестировании отдельной сцены через Play).
/// </summary>
public class GameState : MonoBehaviour
{
    static GameState instance;

    public static GameState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameState>();
                if (instance == null)
                {
                    var go = new GameObject("GameState (auto)");
                    instance = go.AddComponent<GameState>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Глобальный "стоп" для игрового ввода. Пока true — игрок не двигается и не атакует
    /// (используется диалогами Файрена и экраном выбора). См. PlayerMovement.
    /// </summary>
    public static bool InputBlocked { get; set; }

    // Сделанные выборы: ключ развилки -> индекс выбранного варианта.
    readonly Dictionary<string, int> choices = new();

    // Собранные сюжетные предметы ("Осколок судьбы" и т.п.).
    readonly HashSet<string> items = new();

    // Голоса за концовки: индекс концовки (0/1/2) -> сколько выборов на неё указали.
    readonly Dictionary<int, int> endingVotes = new();

    /// <summary>Имя последней посещённой сцены (для "Продолжить игру").</summary>
    public string LastScene { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // Игра начинается с обычного ввода (на случай, если ввод остался заблокирован
        // после перезахода в сцену).
        InputBlocked = false;
    }

    // --- Выборы --------------------------------------------------------------

    /// <param name="choiceKey">Уникальный id развилки, напр. "Scene2_Rasputye".</param>
    /// <param name="optionIndex">Индекс выбранного варианта (0,1,2).</param>
    /// <param name="endingVote">За какую концовку (0/1/2) голосует этот вариант. -1 = не голосует.</param>
    public void RecordChoice(string choiceKey, int optionIndex, int endingVote = -1)
    {
        if (!string.IsNullOrEmpty(choiceKey))
            choices[choiceKey] = optionIndex;

        if (endingVote >= 0)
        {
            endingVotes.TryGetValue(endingVote, out int current);
            endingVotes[endingVote] = current + 1;
        }

        Debug.Log($"GameState: выбор '{choiceKey}' = вариант {optionIndex}, голос за концовку {endingVote}");
    }

    public bool HasChoice(string choiceKey) => choices.ContainsKey(choiceKey);

    public int GetChoice(string choiceKey, int fallback = -1)
        => choices.TryGetValue(choiceKey, out int v) ? v : fallback;

    // --- Предметы ------------------------------------------------------------

    public void AddItem(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId))
            items.Add(itemId);
    }

    public bool HasItem(string itemId) => items.Contains(itemId);

    public IReadOnlyCollection<string> Items => items;

    // --- Концовка ------------------------------------------------------------

    /// <summary>
    /// Возвращает индекс концовки (0/1/2) — ту, за которую набрано больше всего голосов.
    /// При равенстве берётся меньший индекс. Если выборов не было — 0.
    /// </summary>
    public int ResolveEnding()
    {
        // Секретная концовка (индекс 3): собраны все сюжетные предметы
        // (осколок + оба собираемых — значит пройдены Болото и Сад в одном прохождении).
        if (items.Count >= 3)
            return 3;

        int best = 0;
        int bestVotes = -1;
        for (int ending = 0; ending < 3; ending++)
        {
            endingVotes.TryGetValue(ending, out int v);
            if (v > bestVotes)
            {
                bestVotes = v;
                best = ending;
            }
        }
        return best;
    }

    // --- Прогресс ------------------------------------------------------------

    public void SetLastScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            LastScene = sceneName;
    }

    /// <summary>Полный сброс прохождения (для "Новая игра").</summary>
    public void ResetRun()
    {
        choices.Clear();
        items.Clear();
        endingVotes.Clear();
        LastScene = null;
        InputBlocked = false;
    }
}
