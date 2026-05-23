#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class GamePipelineMenu
{
    const string LevelTemplatePath = "Assets/Scenes/_Templates/LevelTemplate.unity";
    const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Game/Setup/Fix Player Sprite")]
    static void FixPlayerSprite()
    {
        ArtImportPipeline.FixPlayerSpriteOnly();
    }

    [MenuItem("Game/Setup/0. Import All Art Packs", false, 0)]
    [MenuItem("Game/Import/Import All Art Packs", false, 1)]
    static void ImportAllArtPacks()
    {
        ArtImportPipeline.ImportAll();
    }

    [MenuItem("Game/Setup/6. Import Hero Animations Only", false, 6)]
    [MenuItem("Game/Import/Hero Only (Animations)", false, 2)]
    static void ImportHeroOnly()
    {
        ArtImportPipeline.ImportHeroOnly();
    }

    [MenuItem("Game/Setup/7. Import Tilesets Only", false, 7)]
    [MenuItem("Game/Import/Tilesets Only", false, 3)]
    static void ImportTilesetsOnly()
    {
        ArtImportPipeline.ImportTilesetsOnly();
    }

    [MenuItem("Game/Setup/1. Create Default Visual Data Assets")]
    static void CreateDefaultVisualData()
    {
        EnsureFolder("Assets/Data/Characters");

        CreateCharacterData("Assets/Data/Characters/PlayerVisualData.asset", GameArtSettings.SortingLayers.Player);
        CreateCharacterData("Assets/Data/Characters/WolfVisualData.asset", GameArtSettings.SortingLayers.Player);
        CreateCharacterData("Assets/Data/Characters/GuardVisualData.asset", GameArtSettings.SortingLayers.Player);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Game: созданы CharacterVisualData в Assets/Data/Characters/");
    }

    [MenuItem("Game/Setup/2. Create Level Template Scene")]
    static void CreateLevelTemplate()
    {
        EnsureFolder("Assets/Scenes/_Templates");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var gridGo = new GameObject("Grid");
        var grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(GameArtSettings.GridCellSize, GameArtSettings.GridCellSize, 0f);

        CreateTilemapLayer(gridGo.transform, "Tilemap_Ground", GameArtSettings.SortingLayers.Ground, 0, false);
        CreateTilemapLayer(gridGo.transform, "Tilemap_Collision", GameArtSettings.SortingLayers.Ground, 0, true);
        CreateTilemapLayer(gridGo.transform, "Tilemap_Decoration", GameArtSettings.SortingLayers.Objects, 1, false);

        new GameObject("Objects");
        new GameObject("NPC");
        new GameObject("PlayerSpawn");

        if (!SaveScene(LevelTemplatePath))
            return;

        Debug.Log($"Game: шаблон уровня сохранён: {LevelTemplatePath}");
    }

    [MenuItem("Game/Setup/3. Create Main Menu Scene")]
    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        var menu = new GameObject("MainMenuController");
        menu.AddComponent<MainMenuController>();

        if (!SaveScene(MainMenuPath))
            return;

        Debug.Log($"Game: сцена меню сохранена: {MainMenuPath}. Добавьте UI-кнопки и привяжите MainMenuController.Play/Quit.");
    }

    [MenuItem("Game/Setup/5. Add Visual Components To NPC Prefabs")]
    static void UpgradeNpcPrefabs()
    {
        UpgradeNpc("Assets/Prefabs/Characters/NPC/WolfEnemy.prefab", "Assets/Data/Characters/WolfVisualData.asset");
        UpgradeNpc("Assets/Prefabs/Characters/NPC/PatrollingGuard.prefab", "Assets/Data/Characters/GuardVisualData.asset");
        Debug.Log("Game: NPC prefabs обновлены (CharacterVisualApplicator).");
    }

    static void UpgradeNpc(string prefabPath, string dataPath)
    {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        var applicator = root.GetComponent<CharacterVisualApplicator>();
        if (applicator == null)
            applicator = root.AddComponent<CharacterVisualApplicator>();

        var data = AssetDatabase.LoadAssetAtPath<CharacterVisualData>(dataPath);
        if (data != null)
        {
            var so = new SerializedObject(applicator);
            so.FindProperty("visualData").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    [MenuItem("Game/Setup/4. Add Visual Components To Player Prefab")]
    static void UpgradePlayerPrefab()
    {
        const string path = "Assets/Prefabs/Characters/Player/Player.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);

        if (root.GetComponent<CharacterAnimationBridge>() == null)
            root.AddComponent<CharacterAnimationBridge>();

        var applicator = root.GetComponent<CharacterVisualApplicator>();
        if (applicator == null)
            applicator = root.AddComponent<CharacterVisualApplicator>();

        var data = AssetDatabase.LoadAssetAtPath<CharacterVisualData>("Assets/Data/Characters/PlayerVisualData.asset");
        if (data != null)
        {
            var so = new SerializedObject(applicator);
            so.FindProperty("visualData").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Game: Player.prefab обновлён (CharacterAnimationBridge + CharacterVisualApplicator).");
    }

    static void CreateCharacterData(string path, string sortingLayer)
    {
        if (AssetDatabase.LoadAssetAtPath<CharacterVisualData>(path) != null)
            return;

        var asset = ScriptableObject.CreateInstance<CharacterVisualData>();
        asset.sortingLayerName = sortingLayer;
        asset.pixelsPerUnit = GameArtSettings.DefaultPixelsPerUnit;
        AssetDatabase.CreateAsset(asset, path);
    }

    static void CreateTilemapLayer(Transform gridParent, string name, string sortingLayer, int order, bool withCollider)
    {
        var go = new GameObject(name);
        go.transform.SetParent(gridParent, false);

        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = order;
        go.AddComponent<Tilemap>();

        if (!withCollider)
            return;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var tileCollider = go.AddComponent<TilemapCollider2D>();
        tileCollider.usedByComposite = true;
        go.AddComponent<CompositeCollider2D>();
    }

    static bool SaveScene(string path)
    {
        if (File.Exists(path) && !EditorUtility.DisplayDialog(
                "Перезаписать сцену?",
                path + " уже существует. Перезаписать?",
                "Да",
                "Отмена"))
            return false;

        return EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
