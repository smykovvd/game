using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Автосборка сцен вертикального среза «Путь через Лес Судеб».
/// Создаёт: Поляну (Сцена 1), Распутье с выбором (Сцена 2), три ветки-заглушки
/// (3A/3B/3C) и экран концовки — со всеми связями, плюс прописывает их в Build Settings.
///
/// Запуск: меню Game → Story → 1. Build Slice Scenes.
/// Безопасно перезапускать: сцены пересоздаются заново.
/// </summary>
public static class StorySceneBuilder
{
    const string StoryDir = "Assets/Scenes/Story";

    const string PlayerPrefab = "Assets/Prefabs/Characters/Player/Player.prefab";
    const string CameraPrefab = "Assets/Prefabs/Objects/Main Camera.prefab";
    const string ChoicePanelPrefab = "Assets/Prefabs/UI/ChoicePanel.prefab";
    const string GameManagerPrefab = "Assets/Prefabs/Objects/GameManager.prefab";
    const string MainMenuScene = "Assets/Scenes/MainMenu.unity";

    // Имена сцен (латиницей — чтобы не было проблем в Build Settings).
    const string S1 = "Scene1_Polyana";
    const string S2 = "Scene2_Rasputye";
    const string S3A = "Scene3A_TropaSumerek";
    const string S3B = "Scene3B_Boloto";
    const string S3C = "Scene3C_HolmEha";
    const string SEnd = "Ending";

    [MenuItem("Game/Story/1. Build Slice Scenes", false, 0)]
    public static void BuildSlice()
    {
        if (!EditorUtility.DisplayDialog("Story Builder",
            "Будут созданы/перезаписаны сцены среза:\n" +
            $"{S1}, {S2}, {S3A}, {S3B}, {S3C}, {SEnd}\nв папке {StoryDir}.\n\nПродолжить?",
            "Да, собрать", "Отмена"))
            return;

        EnsureFolder();

        // Порядок важен: сцена, на которую ссылаются, должна уже существовать на диске.
        SafeBuild("Ending", BuildEnding);
        SafeBuild(S3A, () => BuildBranch(S3A, "Тропа Сумерек", "Сумрак сгущается. Путь ведёт дальше."));
        SafeBuild(S3B, () => BuildBranch(S3B, "Болото Несказанного", "Топь шепчет забытые слова."));
        SafeBuild(S3C, () => BuildBranch(S3C, "Холм Эха", "Эхо повторяет твои шаги."));
        SafeBuild(S2, BuildRasputye);
        SafeBuild(S1, BuildPolyana);

        RegisterBuildScenes();
        PointMainMenuToFirstScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Оставляем открытой Поляну, чтобы сразу нажать Play.
        EditorSceneManager.OpenScene(ScenePath(S1));
        Debug.Log("<color=lime>Story Builder: срез собран. Открыта " + S1 + ". Проверь Build Settings и жми Play.</color>");
        EditorUtility.DisplayDialog("Story Builder", "Готово! Сцены созданы и добавлены в Build Settings.\nОткрыта сцена Поляна — нажми Play.", "Ок");
    }

    static void SafeBuild(string label, Action build)
    {
        try { build(); Debug.Log($"Story Builder: сцена '{label}' собрана."); }
        catch (Exception e) { Debug.LogError($"Story Builder: ошибка при сборке '{label}': {e}"); }
    }

    // ---------- Сцена 1: Поляна ----------
    static void BuildPolyana()
    {
        var scene = NewScene();
        var player = SpawnPlayerAndCamera(Vector3.zero);

        // GameState живёт всю игру — создаём его в стартовой сцене.
        new GameObject("GameState").AddComponent<GameState>();

        // Диалоговая панель Файрена (вступление, играется на старте).
        var fairen = CreateFairenDialog(
            "Файрен",
            new[]
            {
                "Путник… ты ступил в Лес Судеб.",
                "Я — Файрен. Я буду рядом, но выбор всегда твой.",
                "Возьми Осколок судьбы — и ступай к развилке."
            },
            playOnStart: true);

        // «Осколок судьбы».
        CreatePickup("Осколок судьбы", "shard_of_fate", new Vector3(3f, 0f, 0f), fairen,
            "Осколок судьбы теперь с тобой.");

        // Переход на Распутье — только после подбора осколка.
        CreateSceneExit(new Vector3(7f, 0f, 0f), S2, requiredItem: "shard_of_fate",
            lockedMsg: "Сначала возьми Осколок судьбы.", hint: fairen);

        Save(scene, S1);
    }

    // ---------- Сцена 2: Распутье ----------
    static void BuildRasputye()
    {
        var scene = NewScene();
        SpawnPlayerAndCamera(Vector3.zero);

        var choiceManager = CreateChoiceUI();

        // Развилка: три пути, каждый голосует за свою концовку (0/1/2).
        CreateChoiceTrigger(
            new Vector3(4f, 0f, 0f),
            choiceManager,
            options: new[] { "Тропа Сумерек", "Болото Несказанного", "Холм Эха" },
            scenes: new[] { S3A, S3B, S3C },
            votes: new[] { 0, 1, 2 },
            choiceKey: "Scene2_Rasputye",
            hint: "Файрен: Три пути — три судьбы. Выбери: 1, 2 или 3.");

        Save(scene, S2);
    }

    // ---------- Сцены 3A/3B/3C: ветки-заглушки ----------
    static void BuildBranch(string sceneName, string title, string fairenLine)
    {
        var scene = NewScene();
        SpawnPlayerAndCamera(Vector3.zero);

        var fairen = CreateFairenDialog("Файрен", new[] { $"{title}. {fairenLine}", "Иди дальше, к сердцу леса." },
            playOnStart: true);

        // Пока ветки ведут сразу к концовке (позже здесь появятся Сцена 4 → 5 → 6 → 7).
        CreateSceneExit(new Vector3(6f, 0f, 0f), SEnd, requiredItem: "", lockedMsg: "", hint: fairen);

        Save(scene, sceneName);
    }

    // ---------- Экран концовки ----------
    static void BuildEnding()
    {
        var scene = NewScene();

        var canvas = CreateCanvas();
        var bg = AddPanel(canvas.transform, new Color(0.05f, 0.04f, 0.08f, 1f));
        SetRegion(bg.rectTransform, 0, 0, 1, 1, 0);

        var symbol = AddImage(canvas.transform, "Symbol");
        var srt = symbol.rectTransform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.72f);
        srt.sizeDelta = new Vector2(160, 160);
        symbol.enabled = false;

        var text = AddText(canvas.transform, "EndingText", "", 36, Color.white, TextAlignmentOptions.Center);
        SetRegion(text.rectTransform, 0.15f, 0.30f, 0.85f, 0.62f);

        var button = AddButton(canvas.transform, "В меню", new Vector2(220, 64), new Vector2(0, -380));

        var endingGo = new GameObject("EndingScreen");
        var ending = endingGo.AddComponent<EndingScreen>();

        var so = new SerializedObject(ending);
        so.FindProperty("endingText").objectReferenceValue = text;
        so.FindProperty("symbolImage").objectReferenceValue = symbol;
        so.FindProperty("menuSceneName").stringValue = "MainMenu";
        var arr = so.FindProperty("endings");
        arr.arraySize = 3;
        SetEnding(arr, 0, "Свет.\nТы прошёл лес, не потеряв себя.");
        SetEnding(arr, 1, "Тень.\nЛес забрал часть твоей души.");
        SetEnding(arr, 2, "Равновесие.\nТы стал частью леса — ни светом, ни тьмой.");
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(button.onClick, ending.ReturnToMenu);

        Save(scene, SEnd);
    }

    static void SetEnding(SerializedProperty arr, int i, string text)
    {
        var el = arr.GetArrayElementAtIndex(i);
        el.FindPropertyRelative("text").stringValue = text;
        el.FindPropertyRelative("symbol").objectReferenceValue = null;
    }

    // ================= ОБЩИЕ ПОМОЩНИКИ =================

    static Scene NewScene()
        => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    static GameObject SpawnPlayerAndCamera(Vector3 playerPos)
    {
        var playerSrc = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerSrc);
        player.transform.position = playerPos;

        var camSrc = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefab);
        var cam = (GameObject)PrefabUtility.InstantiatePrefab(camSrc);
        cam.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null) follow.target = player.transform;

        return player;
    }

    static FairenDialog CreateFairenDialog(string speaker, string[] lines, bool playOnStart)
    {
        var canvas = CreateCanvas("DialogCanvas");

        var panel = AddPanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
        SetRegion(panel.rectTransform, 0.08f, 0.05f, 0.92f, 0.28f, 0);

        var speakerLabel = AddText(panel.transform, "Speaker", speaker, 28, new Color(1f, 0.72f, 0.25f), TextAlignmentOptions.TopLeft);
        SetRegion(speakerLabel.rectTransform, 0, 0.66f, 1, 1, 16);

        var body = AddText(panel.transform, "Body", "", 26, Color.white, TextAlignmentOptions.TopLeft);
        SetRegion(body.rectTransform, 0, 0, 1, 0.66f, 16);

        var hint = AddText(panel.transform, "Hint", "[Пробел] — далее", 18, new Color(1, 1, 1, 0.5f), TextAlignmentOptions.BottomRight);
        SetRegion(hint.rectTransform, 0.5f, 0, 1, 0.3f, 12);

        var go = new GameObject("FairenDialog");
        var dialog = go.AddComponent<FairenDialog>();
        var so = new SerializedObject(dialog);
        so.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
        so.FindProperty("speakerLabel").objectReferenceValue = speakerLabel;
        so.FindProperty("bodyText").objectReferenceValue = body;
        so.FindProperty("speakerName").stringValue = speaker;
        so.FindProperty("playOnStart").boolValue = playOnStart;
        var linesProp = so.FindProperty("lines");
        linesProp.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
            linesProp.GetArrayElementAtIndex(i).stringValue = lines[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        return dialog;
    }

    static void CreatePickup(string displayName, string itemId, Vector3 pos, FairenDialog dialog, string line)
    {
        var go = new GameObject(displayName);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/Sprites/circle.png");
        sr.color = new Color(0.6f, 0.85f, 1f, 1f);
        if (sr.sprite == null)
            Debug.LogWarning("CreatePickup: спрайт circle.png не загрузился — назначь спрайт предмету вручную.");

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        var pickup = go.AddComponent<StoryItemPickup>();
        var so = new SerializedObject(pickup);
        so.FindProperty("itemId").stringValue = itemId;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("pickOnTouch").boolValue = true;
        so.FindProperty("pickupDialog").objectReferenceValue = dialog;
        so.FindProperty("pickupLine").stringValue = line;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void CreateSceneExit(Vector3 pos, string targetScene, string requiredItem, string lockedMsg, FairenDialog hint)
    {
        var go = new GameObject("SceneExit_" + targetScene);
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 4f);

        var exit = go.AddComponent<SceneExitTrigger>();
        var so = new SerializedObject(exit);
        SetSceneRef(so.FindProperty("targetScene"), targetScene);
        so.FindProperty("requiredItemId").stringValue = requiredItem;
        so.FindProperty("lockedMessage").stringValue = lockedMsg;
        if (hint != null) so.FindProperty("blockedHint").objectReferenceValue = hint;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static ChoiceManager CreateChoiceUI()
    {
        var canvas = CreateCanvas("ChoiceCanvas");

        var panelSrc = AssetDatabase.LoadAssetAtPath<GameObject>(ChoicePanelPrefab);
        var panel = (GameObject)PrefabUtility.InstantiatePrefab(panelSrc, canvas.transform);
        SetRegion(panel.GetComponent<RectTransform>(), 0.2f, 0.25f, 0.8f, 0.75f, 0);

        var gmSrc = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefab);
        var gm = (GameObject)PrefabUtility.InstantiatePrefab(gmSrc);
        var manager = gm.GetComponent<ChoiceManager>();

        var buttonsContainer = panel.transform.Find("ButtonsContainer");
        var closeButton = panel.transform.Find("CloseButton")?.GetComponent<Button>();
        var title = panel.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

        var so = new SerializedObject(manager);
        so.FindProperty("choicePanel").objectReferenceValue = panel;
        if (buttonsContainer != null) so.FindProperty("buttonsContainer").objectReferenceValue = buttonsContainer;
        if (closeButton != null) so.FindProperty("closeButton").objectReferenceValue = closeButton;
        if (title != null) so.FindProperty("hintText").objectReferenceValue = title;
        so.FindProperty("allowClose").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();

        return manager;
    }

    static void CreateChoiceTrigger(Vector3 pos, ChoiceManager manager, string[] options, string[] scenes,
        int[] votes, string choiceKey, string hint)
    {
        var go = new GameObject("ChoiceTrigger");
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 4f);

        var trigger = go.AddComponent<ChoiceTrigger>();
        var so = new SerializedObject(trigger);

        var opt = so.FindProperty("options");
        opt.arraySize = options.Length;
        for (int i = 0; i < options.Length; i++)
            opt.GetArrayElementAtIndex(i).stringValue = options[i];

        var refs = so.FindProperty("sceneReferences");
        refs.arraySize = scenes.Length;
        for (int i = 0; i < scenes.Length; i++)
            SetSceneRef(refs.GetArrayElementAtIndex(i), scenes[i]);

        var v = so.FindProperty("endingVotes");
        v.arraySize = votes.Length;
        for (int i = 0; i < votes.Length; i++)
            v.GetArrayElementAtIndex(i).intValue = votes[i];

        so.FindProperty("fairenHint").stringValue = hint;
        so.FindProperty("choiceKey").stringValue = choiceKey;
        so.FindProperty("choiceManager").objectReferenceValue = manager;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Записывает целевую сцену в SceneReference (и ассет, и имя).
    static void SetSceneRef(SerializedProperty sceneRefProp, string sceneName)
    {
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath(sceneName));
        var assetProp = sceneRefProp.FindPropertyRelative("sceneAsset");
        if (assetProp != null) assetProp.objectReferenceValue = asset;
        sceneRefProp.FindPropertyRelative("sceneName").stringValue = sceneName;
    }

    // ---------- UI-помощники ----------

    static Canvas CreateCanvas(string name = "Canvas")
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }

    static GameObject UIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    static Image AddPanel(Transform parent, Color color)
    {
        var go = UIObject("Panel", parent);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static Image AddImage(Transform parent, string name)
    {
        var go = UIObject(name, parent);
        return go.AddComponent<Image>();
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, int size, Color color, TextAlignmentOptions align)
    {
        var go = UIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    static Button AddButton(Transform parent, string label, Vector2 size, Vector2 anchoredPos)
    {
        var go = UIObject("Button", parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.85f, 0.82f, 0.7f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        var tmp = AddText(go.transform, "Text", label, 28, Color.black, TextAlignmentOptions.Center);
        SetRegion(tmp.rectTransform, 0, 0, 1, 1, 4);
        return btn;
    }

    static void SetRegion(RectTransform rt, float xmin, float ymin, float xmax, float ymax, float pad = 10f)
    {
        rt.anchorMin = new Vector2(xmin, ymin);
        rt.anchorMax = new Vector2(xmax, ymax);
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    // ---------- Файловые помощники ----------

    static string ScenePath(string name) => $"{StoryDir}/{name}.unity";

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(StoryDir))
            AssetDatabase.CreateFolder("Assets/Scenes", "Story");
    }

    static void Save(Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, ScenePath(name));
    }

    static void RegisterBuildScenes()
    {
        var order = new List<string>
        {
            MainMenuScene,
            ScenePath(S1), ScenePath(S2),
            ScenePath(S3A), ScenePath(S3B), ScenePath(S3C),
            ScenePath(SEnd),
        };

        var list = new List<EditorBuildSettingsScene>();
        var seen = new HashSet<string>();
        foreach (var p in order)
        {
            if (File.Exists(p) && seen.Add(p))
                list.Add(new EditorBuildSettingsScene(p, true));
        }
        // Сохраняем ранее добавленные сцены (прототипы), если их ещё нет в списке.
        foreach (var s in EditorBuildSettings.scenes)
            if (seen.Add(s.path))
                list.Add(s);

        EditorBuildSettings.scenes = list.ToArray();
    }

    static void PointMainMenuToFirstScene()
    {
        if (!File.Exists(MainMenuScene)) return;
        var scene = EditorSceneManager.OpenScene(MainMenuScene);
        var controller = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
        if (controller != null)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("firstLevelSceneName").stringValue = S1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
        }
    }
}
