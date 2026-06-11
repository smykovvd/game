using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Диалоговая система духа-лиса Файрена (и любых реплик).
/// Показывает строки по очереди в UI-панели; следующая — по пробелу или клику.
/// Пока идёт диалог, ввод игрока блокируется (GameState.InputBlocked).
/// По завершении вызывает onFinished — туда удобно повесить показ экрана выбора.
/// </summary>
public class FairenDialog : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text speakerLabel;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private string speakerName = "Файрен";

    [Header("Реплики")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;
    [SerializeField] private bool playOnStart = false;

    [Header("Управление")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private bool advanceOnClick = true;
    [SerializeField] private bool blockInputWhilePlaying = true;

    [Header("Событие по завершении")]
    public UnityEvent onFinished;

    readonly List<string> queue = new();
    int index;
    bool playing;

    public bool IsPlaying => playing;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (playOnStart)
            Play();
    }

    void Update()
    {
        if (!playing) return;

        bool advance = Input.GetKeyDown(advanceKey)
                       || (advanceOnClick && Input.GetMouseButtonDown(0));

        if (advance)
            ShowNext();
    }

    /// <summary>Проиграть реплики, заданные в инспекторе.</summary>
    public void Play() => Play(lines);

    /// <summary>Проиграть произвольный набор реплик.</summary>
    public void Play(string[] customLines)
    {
        if (customLines == null || customLines.Length == 0)
        {
            Finish();
            return;
        }

        queue.Clear();
        queue.AddRange(customLines);
        index = -1;
        playing = true;

        if (speakerLabel != null)
            speakerLabel.text = speakerName;
        if (panelRoot != null)
            panelRoot.SetActive(true);
        if (blockInputWhilePlaying)
            GameState.InputBlocked = true;

        ShowNext();
    }

    /// <summary>Показать одну реплику (для коротких подсказок).</summary>
    public void PlaySingle(string message) => Play(new[] { message });

    void ShowNext()
    {
        index++;
        if (index >= queue.Count)
        {
            Finish();
            return;
        }

        if (bodyText != null)
            bodyText.text = queue[index];
    }

    void Finish()
    {
        playing = false;
        if (panelRoot != null)
            panelRoot.SetActive(false);
        if (blockInputWhilePlaying)
            GameState.InputBlocked = false;

        onFinished?.Invoke();
    }
}
