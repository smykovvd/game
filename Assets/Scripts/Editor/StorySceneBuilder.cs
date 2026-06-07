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
using UnityEngine.Tilemaps;
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
    const string S4 = "Scene4_Perekrestok";
    const string S5A = "Scene5A_ObitelTeney";
    const string S5B = "Scene5B_SadPamyati";
    const string S5C = "Scene5C_TropaVetra";
    const string S6 = "Scene6_Koridor";
    const string S7 = "Scene7_SerdceLesa";
    const string SEnd = "Ending";

    // Границы игровой области в клетках (включительно). Используются и для покраски,
    // и для ограничения камеры — камера не выходит за пределы нарисованной сцены.
    const int AreaXMin = -16, AreaXMax = 22, AreaYMin = -11, AreaYMax = 11;

    [MenuItem("Game/Story/1. Build Slice Scenes", false, 0)]
    public static void BuildSlice()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Story Builder",
                "Сначала выйди из режима Play (нажми ▶️ ещё раз, чтобы остановить игру), потом запусти сборку.",
                "Понятно");
            return;
        }

        if (!EditorUtility.DisplayDialog("Story Builder",
            "Будут созданы/перезаписаны все сцены истории:\n" +
            "Поляна → Распутье → 3A/3B/3C → Перекрёсток → 5A/5B/5C → Коридор → Сердце Леса → Концовка\n" +
            $"в папке {StoryDir}.\n\nПродолжить?",
            "Да, собрать", "Отмена"))
            return;

        EnsureFolder();

        // Порядок важен: сцена, на которую ссылаются, должна уже существовать на диске.
        SafeBuild(SEnd, BuildEnding);
        SafeBuild(S7, () => BuildBranch(S7, new[]
        {
            "Ты дошёл. Хотя дороги не было.",
            "Ты всегда доходишь до этого места.",
            "Вот он. Тот, кого ты всё это время обходил. Я стоял там… вместо тебя.",
            "Лес не держал тебя. Ты просто не знал, как уйти. Я знал. Но не смог.",
            "Посмотрим… сможешь ли ты."
        }, SEnd));
        SafeBuild(S6, () => BuildBranch(S6, new[]
        {
            "Теперь всё вместе. Без фильтров. Здесь я обычно замолкаю.",
            "Это не разные пути. Это ты, разбитый на части. Я тоже был таким. Возможно… и остаюсь.",
            "Вот что происходит, когда выборы не забываются. Ты не потерял их — ты их принёс.",
            "Дальше я не смогу идти за тебя. Не потому что не хочу. Потому что… дальше нет меня."
        }, S7, withEnemies: true, combatLine: "Вот что происходит, когда выборы не забываются. Ты не потерял их — ты их принёс."));
        SafeBuild(S5A, () => BuildBranch(S5A, new[]
        {
            "Темнота — это не отсутствие. Это присутствие без имени.",
            "Я дал имена некоторым из них. Это не помогло.",
            "Они не нападали бы, если бы ты их не знал. Не беги. Не борись. Смотри.",
            "Ты не победишь их. Ты перестанешь им сопротивляться — и они исчезнут."
        }, S6, withEnemies: true, combatLine: "Они не нападали бы, если бы ты их не знал. Ты узнаёшь? Или всё ещё делаешь вид?"));
        SafeBuild(S5B, () => BuildBranch(S5B, new[]
        {
            "Красиво, правда? Память всегда такая… сначала.",
            "Я остался здесь однажды. Надолго.",
            "Ты можешь остаться. Здесь никто не стареет. Я проверял.",
            "Отпустить — не значит забыть. Ты начинаешь это понимать."
        }, S6));
        SafeBuild(S5C, () => BuildBranch(S5C, new[]
        {
            "Держись крепче… или отпусти всё сразу.",
            "Я падал здесь. Много раз.",
            "Свобода без опоры — это падение. Я понял это слишком поздно.",
            "Ты стал легче. Вопрос — чего стало меньше? Иногда ответ — «меня»."
        }, S6));
        SafeBuild(S4, () => BuildChoiceScene(S4,
            new[] { "Обитель Теней", "Сад Памяти", "Тропа Ветра" },
            new[] { S5A, S5B, S5C },
            new[] { 0, 1, 2 },
            "Scene4_Perekrestok",
            "Файрен: Теперь ты выбираешь себя. 1, 2 или 3.",
            new[]
            {
                "Теперь ты выбираешь не путь. Ты выбираешь себя.",
                "И это та часть, где всё обычно идёт… иначе.",
                "Ты уже менялся. Просто не признал этого. Ты пытаешься вспомнить… не так ли?"
            }));
        SafeBuild(S3A, () => BuildBranch(S3A, new[]
        {
            "Свет не уходит. Он просто перестаёт быть уверенным.",
            "Здесь ты всегда замедляешься.",
            "Они появляются, когда ты почти решил… но не до конца. Они любят тебя — ты даёшь им повод существовать.",
            "Иди. Ты всегда доходишь. Просто не всегда понимаешь как."
        }, S4, withEnemies: true, combatLine: "Видишь их? Они появляются, когда ты почти решил… но не до конца."));
        SafeBuild(S3B, () => BuildBranch(S3B, new[]
        {
            "Осторожно. Здесь тонут не тела.",
            "Я однажды попытался вытащить кого-то отсюда… он не хотел уходить.",
            "Слышишь? Это слова, которые так и не стали звуком. Некоторые из них — твои.",
            "Иди по суше. В следующий раз они будут громче."
        }, S4));
        SafeBuild(S3C, () => BuildBranch(S3C, new[]
        {
            "Здесь никто не говорит первым.",
            "Ты научился ждать ответа… от себя.",
            "Эхо — это ты. Только чуть раньше. Или чуть честнее.",
            "Ты услышал достаточно… или просто привык к шуму?"
        }, S4));
        SafeBuild(S2, () => BuildChoiceScene(S2,
            new[] { "Тропа Сумерек", "Болото Несказанного", "Холм Эха" },
            new[] { S3A, S3B, S3C },
            new[] { 0, 1, 2 },
            "Scene2_Rasputye",
            "Файрен: Три дороги. И ни одной правильной. 1, 2 или 3.",
            new[]
            {
                "Три дороги. И ни одной правильной.",
                "Раньше ты спросил, есть ли четвёртая.",
                "Ты можешь идти куда угодно. Но не сможешь не идти."
            }));
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

    [MenuItem("Game/Story/2. Paint Forest Tiles", false, 1)]
    public static void PaintForest()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Story Builder", "Сначала выйди из режима Play.", "Понятно");
            return;
        }
        if (!EditorUtility.DisplayDialog("Story Builder",
            "Расписать игровые сцены тайлами Island (трава, тропа, камни, кусты)?\n" +
            "Слои тайлов будут перекрашены заново; остальные объекты не тронуты.",
            "Да, рисовать", "Отмена"))
            return;

        foreach (var s in new[] { S1, S2, S3A, S3B, S3C, S4, S5A, S5B, S5C, S6, S7 })
            SafeBuild(s, () => PaintScene(s));

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(ScenePath(S1));
        EditorUtility.DisplayDialog("Story Builder", "Готово! Лес нарисован. Открыта Поляна — посмотри в Scene/Game.", "Ок");
    }

    [MenuItem("Game/Story/3. Capture Current Scene Tiles", false, 2)]
    public static void CaptureCurrentScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Story Builder", "Сначала выйди из режима Play.", "Понятно");
            return;
        }

        var scene = EditorSceneManager.GetActiveScene();
        var ground = FindTilemap("Tilemap_Ground");
        var collision = FindTilemap("Tilemap_Collision");
        var decor = FindTilemap("Tilemap_Decoration");
        if (ground == null || collision == null || decor == null)
        {
            EditorUtility.DisplayDialog("Story Builder",
                "В открытой сцене нет слоёв Tilemap. Открой игровую сцену (из Assets/Scenes/Story).", "Ок");
            return;
        }

        var lines = new List<string>();
        CaptureLayer(lines, "Ground", ground);
        CaptureLayer(lines, "Collision", collision);
        CaptureLayer(lines, "Decoration", decor);

        Directory.CreateDirectory($"{StoryDir}/Captures");
        File.WriteAllLines(CapturePath(scene.name), lines);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Story Builder",
            $"Слепок сцены «{scene.name}» сохранён ({lines.Count} тайлов).\n" +
            "Теперь кнопка Paint будет рисовать эту сцену именно так, как ты нарисовала.", "Отлично");
    }

    static void CaptureLayer(List<string> lines, string layer, Tilemap map)
    {
        foreach (var pos in map.cellBounds.allPositionsWithin)
        {
            var t = map.GetTile(pos);
            if (t == null) continue;
            string p = AssetDatabase.GetAssetPath(t);
            if (string.IsNullOrEmpty(p)) continue;
            lines.Add($"{layer}|{pos.x}|{pos.y}|{p}");
        }
    }

    static void PaintScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath(sceneName));

        var ground = FindTilemap("Tilemap_Ground");
        var collision = FindTilemap("Tilemap_Collision");
        var decor = FindTilemap("Tilemap_Decoration");
        if (ground == null || collision == null || decor == null)
        {
            Debug.LogError($"PaintScene({sceneName}): не найдены слои Tilemap. Сначала собери сцены (пункт 1).");
            return;
        }

        // Если для сцены есть сохранённый «слепок» (ты нарисовала её руками) — воспроизводим
        // именно его, а не процедурную генерацию.
        if (TryReplayCapture(sceneName, ground, collision, decor))
        {
            FinalizeScene(scene, ground, collision, decor);
            return;
        }

        // Тема сцены: тёмные — Dungeon, Болото — вода Island, остальные — трава Island.
        bool dungeon = sceneName == S3A || sceneName == S5A || sceneName == S6;
        bool swamp = sceneName == S3B;

        TileBase groundTile, pathTile, wallTile;
        TileBase[] decorTiles;
        if (dungeon)
        {
            groundTile = Tile("Dungeon", "Dungeon_24x24_5_4");
            pathTile = groundTile;
            wallTile = Tile("Dungeon", "Dungeon_24x24_4_2");
            decorTiles = new[]
            {
                Tile("Dungeon", "Dungeon_24x24_13_0"), Tile("Dungeon", "Dungeon_24x24_12_5"),
                Tile("Decor", "decor_1_1"), Tile("Decor", "decor_1_0"),
                Tile("Decor", "decor_4_1"), Tile("Decor", "decor_6_3"),
            };
        }
        else if (swamp)
        {
            groundTile = Tile("Island", "Island_24x24_4_7"); // вода
            pathTile = Tile("Island", "Island_24x24_2_0");   // сухая тропа-трава
            wallTile = Tile("Island", "Island_24x24_2_6");   // камни
            decorTiles = new[]
            {
                Tile("Island", "Island_24x24_5_6"), Tile("Island", "Island_24x24_6_6"), // камыши
                Tile("Island", "Island_24x24_4_5"),                                     // кувшинка
                Tile("Island", "Island_24x24_8_0"),                                     // цветы
                Tile("Island", "Island_24x24_1_1"), Tile("Decor", "decor_1_2"),
            };
        }
        else
        {
            groundTile = Tile("Island", "Island_24x24_2_0");
            pathTile = Tile("Island", "Island_24x24_5_2");
            wallTile = Tile("Island", "Island_24x24_2_6");
            decorTiles = new[]
            {
                Tile("Island", "Island_24x24_8_0"), Tile("Island", "Island_24x24_8_1"), // цветы
                Tile("Island", "Island_24x24_8_2"), Tile("Island", "Island_24x24_8_3"), // цветы
                Tile("Island", "Island_24x24_1_1"), Tile("Decor", "decor_1_2"),
                Tile("Decor", "decor_1_1"), Tile("Decor", "decor_4_1"),
            };
        }

        ground.ClearAllTiles();
        collision.ClearAllTiles();
        decor.ClearAllTiles();

        const int xMin = AreaXMin, xMax = AreaXMax, yMin = AreaYMin, yMax = AreaYMax;

        // Земля/пол по всей области.
        for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; y++)
                Set(ground, x, y, groundTile);

        // Поляна и сцены выбора рисуются развилкой (всегда вправо), остальные — прямой
        // тропой в своём направлении (разные сцены — разные стороны).
        bool crossroads = sceneName == S1 || sceneName == S2 || sceneName == S4;
        // Перегородки-«комнаты» оставляем только на травяных линейных сценах.
        bool partitions = !crossroads && !dungeon && !swamp;
        var (fwd, side, len) = LayoutFor(sceneName);
        int p1 = Mathf.RoundToInt(len * 0.3f);
        int p2 = Mathf.RoundToInt(len * 0.55f);
        int p3 = Mathf.RoundToInt(len * 0.8f);

        if (crossroads)
        {
            const int fork = 4;
            for (int x = xMin; x <= fork; x++) { Set(ground, x, 0, pathTile); Set(ground, x, -1, pathTile); }
            for (int y = -6; y <= 6; y++) Set(ground, fork, y, pathTile);
            for (int x = fork; x <= xMax; x++)
            {
                Set(ground, x, 6, pathTile); Set(ground, x, 5, pathTile);
                Set(ground, x, 0, pathTile); Set(ground, x, -1, pathTile);
                Set(ground, x, -6, pathTile); Set(ground, x, -5, pathTile);
            }
        }
        else
        {
            // Двухклеточная тропа вдоль направления — от входа до выхода.
            for (int t = -2; t <= len; t++)
            {
                var a = fwd * t;
                var b = a - side;
                Set(ground, a.x, a.y, pathTile);
                Set(ground, b.x, b.y, pathTile);
            }
        }

        // Стены по всему периметру.
        for (int x = xMin; x <= xMax; x++) { Set(collision, x, yMax, wallTile); Set(collision, x, yMin, wallTile); }
        for (int y = yMin; y <= yMax; y++) { Set(collision, xMin, y, wallTile); Set(collision, xMax, y, wallTile); }

        // Перегородки-«комнаты» поперёк направления, с проходом по тропе; плюс глухая
        // стена позади игрока — чтобы направляла вперёд.
        if (!crossroads)
        {
            int sLo = side.x != 0 ? xMin + 1 : yMin + 1;
            int sHi = side.x != 0 ? xMax - 1 : yMax - 1;

            if (partitions)
                foreach (int p in new[] { p1, p2, p3 })
                    for (int s = sLo; s <= sHi; s++)
                    {
                        if (s >= -1 && s <= 1) continue; // проход по тропе
                        var c = fwd * p + side * s;
                        Set(collision, c.x, c.y, wallTile);
                    }

            // Глухая стена за спиной — направляет вперёд (на всех линейных сценах).
            for (int s = sLo; s <= sHi; s++)
            {
                var c = fwd * -3 + side * s;
                Set(collision, c.x, c.y, wallTile);
            }
        }

        // Декор структурными группами (одинаковые пропсы рядом), не на тропе/рукавах/перегородках.
        bool Blocked(int x, int y)
        {
            if (crossroads)
                return (y >= -1 && y <= 1) || y == 5 || y == 6 || y == -5 || y == -6 || x == 4;
            int fc = x * fwd.x + y * fwd.y;
            int sc = x * side.x + y * side.y;
            if (sc >= -1 && sc <= 1) return true;
            if (partitions && (fc == p1 || fc == p2 || fc == p3)) return true;
            return false;
        }

        var clusterOffsets = new[]
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1),
            new Vector2Int(1, 1), new Vector2Int(2, 0), new Vector2Int(0, -1),
        };
        for (int cx = xMin + 2; cx <= xMax - 2; cx += 3)
            for (int cy = yMin + 2; cy <= yMax - 2; cy += 3)
            {
                int h = CellHash(cx, cy);
                if (h % 100 >= 50) continue;                        // ~50% узлов — группа
                var t = decorTiles[(h / 100) % decorTiles.Length];  // один тип на всю группу
                if (t == null) continue;
                int count = 1 + (h / 7) % 3;                        // 1..3 штуки рядом
                int placed = 0;
                foreach (var off in clusterOffsets)
                {
                    if (placed >= count) break;
                    int x = cx + off.x, y = cy + off.y;
                    if (x < xMin + 1 || x > xMax - 1 || y < yMin + 1 || y > yMax - 1) continue;
                    if (Blocked(x, y)) continue;
                    Set(decor, x, y, t);
                    placed++;
                }
            }

        FinalizeScene(scene, ground, collision, decor);
    }

    static void FinalizeScene(Scene scene, Tilemap ground, Tilemap collision, Tilemap decor)
    {
        // Нормализуем размер сюжетных предметов (осколок огромный из-за крупной картинки).
        foreach (var pickup in UnityEngine.Object.FindObjectsByType<StoryItemPickup>(FindObjectsSortMode.None))
            pickup.transform.localScale = new Vector3(0.04f, 0.04f, 1f);

        EditorUtility.SetDirty(ground);
        EditorUtility.SetDirty(collision);
        EditorUtility.SetDirty(decor);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Путь к файлу-«слепку» тайлов сцены.
    static string CapturePath(string sceneName) => $"{StoryDir}/Captures/{sceneName}.txt";

    // Воспроизводит сохранённый слепок тайлов (если он есть). true — если воспроизвели.
    static bool TryReplayCapture(string sceneName, Tilemap ground, Tilemap collision, Tilemap decor)
    {
        string path = CapturePath(sceneName);
        if (!File.Exists(path)) return false;

        ground.ClearAllTiles();
        collision.ClearAllTiles();
        decor.ClearAllTiles();

        foreach (var raw in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var parts = raw.Split('|');
            if (parts.Length != 4) continue;
            var map = parts[0] == "Ground" ? ground : parts[0] == "Collision" ? collision : decor;
            if (!int.TryParse(parts[1], out int x) || !int.TryParse(parts[2], out int y)) continue;
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(parts[3]);
            if (tile != null) map.SetTile(new Vector3Int(x, y, 0), tile);
        }
        Debug.Log($"PaintScene: '{sceneName}' воспроизведена из слепка.");
        return true;
    }

    static void Set(Tilemap map, int x, int y, TileBase tile)
        => map.SetTile(new Vector3Int(x, y, 0), tile);

    // Направление сцены: куда идти от входа к выходу + перпендикуляр + длина.
    // Линейные сцены смотрят в разные стороны для разнообразия.
    static (Vector2Int fwd, Vector2Int side, int len) LayoutFor(string scene)
    {
        Vector2Int right = new Vector2Int(1, 0), left = new Vector2Int(-1, 0);
        Vector2Int up = new Vector2Int(0, 1), down = new Vector2Int(0, -1);
        Vector2Int sideForHoriz = new Vector2Int(0, 1);  // перпендикуляр для горизонтального движения
        Vector2Int sideForVert = new Vector2Int(1, 0);   // перпендикуляр для вертикального

        switch (scene)
        {
            case S3B: return (up, sideForVert, 9);    // Болото — вверх
            case S3C: return (down, sideForVert, 9);  // Холм Эха — вниз
            case S5A: return (left, sideForHoriz, 14); // Обитель Теней — влево
            case S5B: return (up, sideForVert, 9);    // Сад Памяти — вверх
            case S6: return (down, sideForVert, 9);   // Коридор — вниз
            default: return (right, sideForHoriz, 19); // вправо (3A, 5C, 7 и сцены выбора)
        }
    }

    // Псевдослучайный хэш клетки — для неравномерного разброса декора.
    static int CellHash(int x, int y)
    {
        unchecked
        {
            int h = (x * 73856093) ^ (y * 19349663) ^ 0x5bd1e995;
            h ^= h >> 13; h *= 0x27d4eb2f; h ^= h >> 16;
            return h & 0x7fffffff;
        }
    }

    static Tilemap FindTilemap(string name)
    {
        foreach (var tm in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (tm.name == name) return tm;
        return null;
    }

    static TileBase Tile(string folder, string assetName)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Graphics/Environment/Tiles/{folder}/{assetName}.asset");
        if (tile == null) Debug.LogError($"PaintScene: тайл не найден: {folder}/{assetName}");
        return tile;
    }

    // ---------- Сцена 1: Поляна ----------
    static void BuildPolyana()
    {
        var scene = NewScene();
        CreateGrid();
        var player = SpawnPlayerAndCamera(Vector3.zero);

        // GameState живёт всю игру — создаём его в стартовой сцене.
        new GameObject("GameState").AddComponent<GameState>();

        // Диалоговая панель Файрена (вступление, играется на старте).
        var fairen = CreateFairenDialog(
            "Файрен",
            new[]
            {
                "…О. Ты снова здесь.",
                "В прошлый раз ты проснулся быстрее.",
                "Не спеши вставать. Мир никуда не денется. В отличие от памяти.",
                "Ты ищешь имя? Здесь они не задерживаются. Я пробовал хранить их… не получилось.",
                "Шаги без прошлого звучат особенно громко. Иди — где-то здесь лежит Осколок судьбы."
            },
            playOnStart: true);

        // «Осколок судьбы» — спрятан в стороне от тропы, его нужно поискать.
        CreatePickup("Осколок судьбы", "shard_of_fate", new Vector3(-4f, 8f, 0f), fairen,
            "Нашёл. Хотя… такие вещи не находят. Это не предмет — это вопрос, на который ты уже отвечал.");

        // Переход на Распутье в дальнем правом краю — только после подбора осколка.
        CreateSceneExit(new Vector3(19f, 0f, 0f), S2, requiredItem: "shard_of_fate",
            lockedMsg: "Сначала найди Осколок судьбы.", hint: fairen);

        Save(scene, S1);
    }

    // ---------- Сцена выбора (Распутье, Перекрёсток) ----------
    static void BuildChoiceScene(string sceneName, string[] options, string[] scenes, int[] votes,
        string choiceKey, string hint, string[] introLines = null)
    {
        var scene = NewScene();
        CreateGrid();
        SpawnPlayerAndCamera(Vector3.zero);

        if (introLines != null && introLines.Length > 0)
            CreateFairenDialog("Файрен", introLines, playOnStart: true);

        var choiceManager = CreateChoiceUI();
        CreateChoiceTrigger(new Vector3(4f, 0f, 0f), choiceManager, options, scenes, votes, choiceKey, hint);

        Save(scene, sceneName);
    }

    // ---------- Линейная сцена-ветка (3A/B/C, 5A/B/C, Коридор, Сердце Леса) ----------
    static void BuildBranch(string sceneName, string[] lines, string target,
        bool withEnemies = false, string combatLine = null)
    {
        var scene = NewScene();
        CreateGrid();
        SpawnPlayerAndCamera(Vector3.zero);

        var fairen = CreateFairenDialog("Файрен", lines, playOnStart: true);

        var (fwd, side, len) = LayoutFor(sceneName);
        int p1 = Mathf.RoundToInt(len * 0.3f);
        int p2 = Mathf.RoundToInt(len * 0.55f);
        int p3 = Mathf.RoundToInt(len * 0.8f);
        Vector3 At(int forward, int sideOff)
        {
            var c = fwd * forward + side * sideOff;
            return new Vector3(c.x, c.y, 0f);
        }

        // Выход в дальнем конце — через все «комнаты», в направлении сцены.
        CreateSceneExit(At(len, 0), target, requiredItem: "", lockedMsg: "", hint: fairen);

        if (withEnemies)
        {
            // При входе — боевая реплика Файрена.
            if (!string.IsNullOrEmpty(combatLine))
                CreateLineTrigger(At(Mathf.RoundToInt(len * 0.15f), 0), fairen, combatLine);

            // Тени-противники — в «комнатах» между перегородками, в стороне от тропы.
            SpawnEnemy("WolfEnemy", At((p1 + p2) / 2, 3));
            SpawnEnemy("WolfEnemy", At((p2 + p3) / 2, -3));
            SpawnEnemy("PatrollingGuard", At((p3 + len) / 2, 3));
        }

        Save(scene, sceneName);
    }

    static void SpawnEnemy(string prefabName, Vector3 pos)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Characters/NPC/{prefabName}.prefab");
        if (src == null)
        {
            Debug.LogError($"SpawnEnemy: префаб не найден: {prefabName}");
            return;
        }
        var e = (GameObject)PrefabUtility.InstantiatePrefab(src);
        e.transform.position = pos;

        // Без EnemyHealth по врагу нельзя попасть — добавляем, если нет.
        if (e.GetComponent<EnemyHealth>() == null)
            e.AddComponent<EnemyHealth>();

        // Чтобы кинематический враг ловил удар-триггер.
        var rb = e.GetComponent<Rigidbody2D>();
        if (rb != null) rb.useFullKinematicContacts = true;

        // Затемняем под «тень».
        var sr = e.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.14f, 0.10f, 0.22f, 0.92f);
    }

    static void CreateLineTrigger(Vector3 pos, FairenDialog dialog, string line)
    {
        var go = new GameObject("FairenLineTrigger");
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 8f);

        var t = go.AddComponent<FairenLineTrigger>();
        var so = new SerializedObject(t);
        so.FindProperty("dialog").objectReferenceValue = dialog;
        so.FindProperty("line").stringValue = line;
        so.ApplyModifiedPropertiesWithoutUndo();
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

    // Холст для рисования тайлами: Grid + три слоя (земля, стены, декор).
    static void CreateGrid()
    {
        var gridGo = new GameObject("Grid", typeof(Grid));
        gridGo.GetComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);

        CreateTilemapLayer(gridGo.transform, "Tilemap_Ground", 0, false);
        CreateTilemapLayer(gridGo.transform, "Tilemap_Collision", 1, true);
        CreateTilemapLayer(gridGo.transform, "Tilemap_Decoration", 2, false);
    }

    static void CreateTilemapLayer(Transform parent, string name, int sortingOrder, bool withCollider)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(parent, false);
        // Слой сортировки Default — рисуется под игроком (он на верхнем слое).
        go.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;
        if (withCollider)
            go.AddComponent<TilemapCollider2D>();
    }

    static GameObject SpawnPlayerAndCamera(Vector3 playerPos)
    {
        var playerSrc = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerSrc);
        player.transform.position = playerPos;

        var camSrc = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefab);
        var cam = (GameObject)PrefabUtility.InstantiatePrefab(camSrc);
        cam.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.target = player.transform;
            // Камера не выходит за пределы нарисованной области (в мировых координатах).
            follow.useBounds = true;
            follow.boundsMin = new Vector2(AreaXMin, AreaYMin);
            follow.boundsMax = new Vector2(AreaXMax + 1, AreaYMax + 1);
        }

        EnsureAttackHitbox(player);
        return player;
    }

    // Игроку нужен дочерний триггер-хитбокс, иначе атака (пробел) ни по кому не попадает.
    static void EnsureAttackHitbox(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm == null || pm.attackHitbox != null) return;

        var hb = new GameObject("AttackHitbox");
        hb.transform.SetParent(player.transform, false);
        var col = hb.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 1.2f);
        hb.AddComponent<AttackHitbox>();
        hb.SetActive(false);                 // включается только на время удара
        pm.attackHitbox = hb;                // коллайдер наследует Rigidbody2D игрока → триггеры срабатывают
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
        // circle.png — 1920px при PPU 100 (~19 юнитов), поэтому сильно уменьшаем.
        go.transform.localScale = new Vector3(0.04f, 0.04f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/Sprites/circle.png");
        sr.sortingOrder = 10;
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
        SetRegion(panel.GetComponent<RectTransform>(), 0.18f, 0.2f, 0.82f, 0.8f, 0);

        var gmSrc = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefab);
        var gm = (GameObject)PrefabUtility.InstantiatePrefab(gmSrc);
        var manager = gm.GetComponent<ChoiceManager>();

        var buttonsContainer = panel.transform.Find("ButtonsContainer");
        var closeButton = panel.transform.Find("CloseButton")?.GetComponent<Button>();
        var title = panel.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

        // Чиним раскладку кнопок: у контейнера был нулевой размер и горизонтальная
        // раскладка — варианты налезали друг на друга. Делаем вертикальный список
        // во всю ширину панели с отступами.
        if (buttonsContainer != null)
        {
            SetRegion(buttonsContainer.GetComponent<RectTransform>(), 0.05f, 0.06f, 0.95f, 0.62f, 8f);

            var oldLayout = buttonsContainer.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (oldLayout != null) UnityEngine.Object.DestroyImmediate(oldLayout);

            var v = buttonsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 12;
            v.padding = new RectOffset(10, 10, 10, 10);
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = true;
        }

        // Подсказку Файрена (Title) растягиваем по верху панели и центрируем.
        if (title != null)
        {
            SetRegion(title.rectTransform, 0.05f, 0.64f, 0.95f, 0.96f, 6f);
            title.alignment = TextAlignmentOptions.Center;
            title.enableAutoSizing = false;
            title.fontSize = 30;
        }

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
            ScenePath(S4),
            ScenePath(S5A), ScenePath(S5B), ScenePath(S5C),
            ScenePath(S6), ScenePath(S7),
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
