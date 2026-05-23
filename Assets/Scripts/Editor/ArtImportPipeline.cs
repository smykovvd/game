using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using AnimatorControllerAsset = UnityEditor.Animations.AnimatorController;

/// <summary>
/// Импорт RPG_Hero, 32rogues, P_P_FREE_RPG_TILESET из Assets/Graphics/_Source.
/// Запуск: Game → Import → Import All Art Packs.
/// </summary>
public static class ArtImportPipeline
{
    const string HeroRoot = "Assets/Graphics/Characters/Player/Sprites/RPG_Hero";
    const string RoguesRoot = "Assets/Graphics/_Source/32rogues";
    const string PpRoot = "Assets/Graphics/_Source/P_P_FREE_RPG_TILESET";

    const string AnimFolder = "Assets/Graphics/Characters/Player/Animations";
    const string ControllerPath = "Assets/Graphics/Characters/Player/Animators/Player.controller";
    const string TilesDungeon = "Assets/Graphics/Environment/Tiles/Dungeon";
    const string TilesIsland = "Assets/Graphics/Environment/Tiles/Island";
    const string Tiles32Rogues = "Assets/Graphics/Environment/Tiles/32rogues";

    const string PlayerVisualPath = "Assets/Data/Characters/PlayerVisualData.asset";
    const string WolfVisualPath = "Assets/Data/Characters/WolfVisualData.asset";
    const string GuardVisualPath = "Assets/Data/Characters/GuardVisualData.asset";
    const string DungeonTilesetDataPath = "Assets/Data/Tiles/DungeonTilesetData.asset";

    public static void FixPlayerSpriteOnly()
    {
        var sprite = GetDefaultHeroSprite();
        if (sprite == null)
        {
            Debug.LogError("ArtImportPipeline: не найден спрайт idle_down_40x40_0. Сначала Import All Art Packs.");
            return;
        }

        var data = AssetDatabase.LoadAssetAtPath<CharacterVisualData>(PlayerVisualPath);
        if (data != null)
        {
            data.defaultSprite = sprite;
            data.pixelsPerUnit = GameArtSettings.HeroPixelsPerUnit;
            EditorUtility.SetDirty(data);
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorControllerAsset>(ControllerPath);
        ApplyPlayerPrefab(controller, sprite, data);
        AssetDatabase.SaveAssets();
        Debug.Log($"ArtImportPipeline: спрайт игрока назначен ({sprite.name}).");
    }

    public static void ImportAll()
    {
        EnsureFolders();
        ImportHeroStrips();
        ImportSpriteSheets();
        AssetDatabase.Refresh();

        var idleDown = CreateDirectionClips("idle", 6f, true);
        var runDown = CreateDirectionClips("run", 10f, true);
        var attackDown = CreateDirectionClips("attack", 12f, false);

        var controller = BuildPlayerAnimator(idleDown, runDown, attackDown);
        CreateTileAssets();
        AssignVisualDataAndPrefabs(controller, GetDefaultHeroSprite());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArtImportPipeline: импорт завершён. Проверьте Player.prefab и палитру: Window → 2D → Tile Palette.");
    }

    public static void ImportHeroOnly()
    {
        EnsureFolders();
        ImportHeroStrips();
        AssetDatabase.Refresh();
        var idleDown = CreateDirectionClips("idle", 6f, true);
        var runDown = CreateDirectionClips("run", 10f, true);
        var attackDown = CreateDirectionClips("attack", 12f, false);
        var controller = BuildPlayerAnimator(idleDown, runDown, attackDown);
        AssignPlayerVisual(controller, GetDefaultHeroSprite());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void ImportTilesetsOnly()
    {
        EnsureFolders();
        ImportSpriteSheets();
        AssetDatabase.Refresh();
        CreateTileAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void EnsureFolders()
    {
        string[] folders =
        {
            HeroRoot, RoguesRoot, PpRoot, AnimFolder,
            "Assets/Graphics/Characters/Player/Animators",
            TilesDungeon, TilesIsland, Tiles32Rogues,
            "Assets/Data/Tiles"
        };

        foreach (var f in folders)
            EnsureFolder(f);
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

    static void ImportHeroStrips()
    {
        if (!AssetDatabase.IsValidFolder(HeroRoot))
        {
            Debug.LogError($"ArtImportPipeline: нет папки {HeroRoot}. Скопируйте RPG_Hero в Assets/Graphics/Characters/Player/Sprites/RPG_Hero");
            return;
        }

        foreach (var path in Directory.GetFiles(HeroRoot, "*.png", SearchOption.AllDirectories))
        {
            var asset = path.Replace("\\", "/");
            if (asset.StartsWith("Assets/"))
                SliceHorizontalStrip(asset, 40, 40, GameArtSettings.HeroPixelsPerUnit);
        }
    }

    static void ImportSpriteSheets()
    {
        SliceGrid($"{RoguesRoot}/monsters.png", 32, 32, GameArtSettings.RoguesPixelsPerUnit);
        SliceGrid($"{RoguesRoot}/rogues.png", 32, 32, GameArtSettings.RoguesPixelsPerUnit);
        SliceGrid($"{RoguesRoot}/tiles.png", 32, 32, GameArtSettings.RoguesPixelsPerUnit);
        SliceGrid($"{RoguesRoot}/items.png", 32, 32, GameArtSettings.RoguesPixelsPerUnit);

        SliceGrid($"{PpRoot}/Dungeon_24x24.png", 24, 24, GameArtSettings.TilesetPixelsPerUnit);
        SliceGrid($"{PpRoot}/Island_24x24.png", 24, 24, GameArtSettings.TilesetPixelsPerUnit);
        SliceGrid($"{PpRoot}/decor.png", 24, 24, GameArtSettings.TilesetPixelsPerUnit);
    }

    static void SliceHorizontalStrip(string assetPath, int frameW, int frameH, int ppu)
    {
        if (!File.Exists(assetPath))
            return;

        var importer = GetSpriteImporter(assetPath, ppu);
        importer.GetSourceTextureWidthAndHeight(out var w, out var h);
        var baseName = Path.GetFileNameWithoutExtension(assetPath);
        var metas = new List<SpriteMetaData>();
        var count = w / frameW;

        for (var i = 0; i < count; i++)
        {
            metas.Add(new SpriteMetaData
            {
                name = $"{baseName}_{i}",
                rect = new Rect(i * frameW, 0, frameW, frameH),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });
        }

        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();
    }

    static void SliceGrid(string assetPath, int cellW, int cellH, int ppu)
    {
        if (!File.Exists(assetPath))
        {
            Debug.LogWarning($"ArtImportPipeline: файл не найден {assetPath}");
            return;
        }

        var importer = GetSpriteImporter(assetPath, ppu);
        importer.GetSourceTextureWidthAndHeight(out var texW, out var texH);
        var baseName = Path.GetFileNameWithoutExtension(assetPath);
        var cols = texW / cellW;
        var rows = texH / cellH;
        var metas = new List<SpriteMetaData>();

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var unityRow = rows - 1 - row;
                metas.Add(new SpriteMetaData
                {
                    name = $"{baseName}_{col}_{row}",
                    rect = new Rect(col * cellW, unityRow * cellH, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }
        }

        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();
    }

    static TextureImporter GetSpriteImporter(string assetPath, int ppu)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = ppu;
        return importer;
    }

    struct DirectionClips
    {
        public AnimationClip Down;
        public AnimationClip Up;
        public AnimationClip Left;
        public AnimationClip Right;
    }

    static DirectionClips CreateDirectionClips(string action, float fps, bool loop)
    {
        return new DirectionClips
        {
            Down = CreateStripClip($"{AnimFolder}/Player_{action}_Down.anim", $"{HeroRoot}/{action}/{action}_down_40x40.png", fps, loop),
            Up = CreateStripClip($"{AnimFolder}/Player_{action}_Up.anim", $"{HeroRoot}/{action}/{action}_up_40x40.png", fps, loop),
            Left = CreateStripClip($"{AnimFolder}/Player_{action}_Left.anim", $"{HeroRoot}/{action}/{action}_left_40x40.png", fps, loop),
            Right = CreateStripClip($"{AnimFolder}/Player_{action}_Right.anim", $"{HeroRoot}/{action}/{action}_right_40x40.png", fps, loop)
        };
    }

    static AnimationClip CreateStripClip(string clipPath, string texturePath, float fps, bool loop)
    {
        var sprites = LoadStripSprites(texturePath);
        if (sprites.Length == 0)
        {
            Debug.LogWarning($"ArtImportPipeline: нет кадров для {texturePath}");
            return null;
        }

        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(clipPath);

        var clip = new AnimationClip { frameRate = fps };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (var i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    static Sprite[] LoadStripSprites(string texturePath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .OrderBy(s => s.rect.x)
            .ToArray();
    }

    static Sprite GetDefaultHeroSprite()
    {
        var texturePath = HeroRoot + "/idle/idle_down_40x40.png";
        var byName = AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .FirstOrDefault(s => s.name == "idle_down_40x40_0");

        if (byName != null)
            return byName;

        var sprites = LoadStripSprites(texturePath);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    static Sprite LoadGridSprite(string texturePath, int col, int rowFromTop)
    {
        var name = $"{Path.GetFileNameWithoutExtension(texturePath)}_{col}_{rowFromTop}";
        return AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .FirstOrDefault(s => s.name == name);
    }

    static AnimatorControllerAsset BuildPlayerAnimator(DirectionClips idle, DirectionClips run, DirectionClips attack)
    {
        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorControllerAsset.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter(GameArtSettings.AnimatorParameters.Speed, AnimatorControllerParameterType.Float);
        controller.AddParameter(GameArtSettings.AnimatorParameters.MoveX, AnimatorControllerParameterType.Float);
        controller.AddParameter(GameArtSettings.AnimatorParameters.MoveY, AnimatorControllerParameterType.Float);
        controller.AddParameter(GameArtSettings.AnimatorParameters.IsAttacking, AnimatorControllerParameterType.Bool);
        controller.AddParameter(GameArtSettings.AnimatorParameters.AttackTrigger, AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;
        var idleTree = CreateDirectionBlendTree("IdleBlend", idle);
        var runTree = CreateDirectionBlendTree("RunBlend", run);
        var attackTree = CreateDirectionBlendTree("AttackBlend", attack);

        AssetDatabase.AddObjectToAsset(idleTree, controller);
        AssetDatabase.AddObjectToAsset(runTree, controller);
        AssetDatabase.AddObjectToAsset(attackTree, controller);

        var idleState = sm.AddState("Idle", new Vector3(300, 0, 0));
        idleState.motion = idleTree;
        var runState = sm.AddState("Run", new Vector3(300, 120, 0));
        runState.motion = runTree;
        var attackState = sm.AddState("Attack", new Vector3(550, 60, 0));
        attackState.motion = attackTree;

        sm.defaultState = idleState;

        var toRun = idleState.AddTransition(runState);
        toRun.AddCondition(AnimatorConditionMode.Greater, 0.01f, GameArtSettings.AnimatorParameters.Speed);
        toRun.duration = 0.05f;

        var toIdle = runState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, GameArtSettings.AnimatorParameters.Speed);
        toIdle.duration = 0.05f;

        var toAttack = sm.AddAnyStateTransition(attackState);
        toAttack.AddCondition(AnimatorConditionMode.If, 0, GameArtSettings.AnimatorParameters.AttackTrigger);
        toAttack.duration = 0.05f;

        var attackEnd = attackState.AddTransition(idleState);
        attackEnd.hasExitTime = true;
        attackEnd.exitTime = 0.9f;
        attackEnd.duration = 0.05f;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    static BlendTree CreateDirectionBlendTree(string treeName, DirectionClips clips)
    {
        var tree = new BlendTree
        {
            name = treeName,
            blendType = BlendTreeType.SimpleDirectional2D,
            blendParameter = GameArtSettings.AnimatorParameters.MoveX,
            blendParameterY = GameArtSettings.AnimatorParameters.MoveY,
            useAutomaticThresholds = false
        };

        if (clips.Down != null) tree.AddChild(clips.Down, new Vector2(0f, -1f));
        if (clips.Up != null) tree.AddChild(clips.Up, new Vector2(0f, 1f));
        if (clips.Left != null) tree.AddChild(clips.Left, new Vector2(-1f, 0f));
        if (clips.Right != null) tree.AddChild(clips.Right, new Vector2(1f, 0f));

        return tree;
    }

    static void CreateTileAssets()
    {
        CreateTilesFromTexture($"{PpRoot}/Dungeon_24x24.png", TilesDungeon);
        CreateTilesFromTexture($"{PpRoot}/Island_24x24.png", TilesIsland);
        CreateTilesFromTexture($"{RoguesRoot}/tiles.png", Tiles32Rogues);
    }

    static void CreateTilesFromTexture(string texturePath, string folder)
    {
        if (!File.Exists(texturePath))
            return;

        foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>())
        {
            var tilePath = $"{folder}/{sprite.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (existing != null)
                AssetDatabase.DeleteAsset(tilePath);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, tilePath);
        }
    }

    static void AssignVisualDataAndPrefabs(AnimatorControllerAsset controller, Sprite defaultSprite)
    {
        AssignPlayerVisual(controller, defaultSprite);
        AssignNpcVisual(WolfVisualPath, "Assets/Prefabs/Characters/NPC/WolfEnemy.prefab",
            $"{RoguesRoot}/monsters.png", 0, 0);
        AssignNpcVisual(GuardVisualPath, "Assets/Prefabs/Characters/NPC/PatrollingGuard.prefab",
            $"{RoguesRoot}/rogues.png", 3, 0);

        var tileset = AssetDatabase.LoadAssetAtPath<TilesetData>(DungeonTilesetDataPath);
        if (tileset == null)
        {
            tileset = ScriptableObject.CreateInstance<TilesetData>();
            tileset.biomeId = "dungeon";
            tileset.displayName = "Dungeon 24x24";
            tileset.pixelsPerUnit = GameArtSettings.TilesetPixelsPerUnit;
            tileset.cellSize = 1f;
            AssetDatabase.CreateAsset(tileset, DungeonTilesetDataPath);
        }
    }

    static void AssignPlayerVisual(AnimatorControllerAsset controller, Sprite defaultSprite)
    {
        var data = AssetDatabase.LoadAssetAtPath<CharacterVisualData>(PlayerVisualPath);
        if (data == null)
            return;

        data.animatorController = controller;
        data.defaultSprite = defaultSprite;
        data.pixelsPerUnit = GameArtSettings.HeroPixelsPerUnit;
        data.sortingLayerName = GameArtSettings.SortingLayers.Player;
        EditorUtility.SetDirty(data);

        ApplyPlayerPrefab(controller, defaultSprite, data);
    }

    static void ApplyPlayerPrefab(AnimatorControllerAsset controller, Sprite defaultSprite, CharacterVisualData data)
    {
        const string prefabPath = "Assets/Prefabs/Characters/Player/Player.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            if (root.GetComponent<CharacterAnimationBridge>() == null)
                root.AddComponent<CharacterAnimationBridge>();

            var applicator = root.GetComponent<CharacterVisualApplicator>() ?? root.AddComponent<CharacterVisualApplicator>();
            var appSo = new SerializedObject(applicator);
            appSo.FindProperty("visualData").objectReferenceValue = data;
            appSo.ApplyModifiedPropertiesWithoutUndo();

            var playerAnimator = EnsurePlayerAnimator(root);
            if (playerAnimator == null)
            {
                Debug.LogError("ArtImportPipeline: не удалось добавить Animator на Player. Сохраните префаб вручную.");
                return;
            }

            var animatorSo = new SerializedObject(playerAnimator);
            animatorSo.FindProperty("m_Controller").objectReferenceValue = controller;
            animatorSo.FindProperty("m_UpdateMode").intValue = (int)AnimatorUpdateMode.Normal;
            animatorSo.ApplyModifiedPropertiesWithoutUndo();

            var bridge = root.GetComponent<CharacterAnimationBridge>();
            var bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("animator").objectReferenceValue = playerAnimator;
            bridgeSo.FindProperty("flipTarget").objectReferenceValue = root.transform;
            bridgeSo.ApplyModifiedPropertiesWithoutUndo();

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null && defaultSprite != null)
            {
                sr.sprite = defaultSprite;
                sr.color = Color.white;
                sr.maskInteraction = SpriteMaskInteraction.None;
            }

            var maskChild = root.transform.Find("VisibilityMask");
            if (maskChild != null)
                maskChild.gameObject.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>
    /// Явно UnityEngine.Animator — иначе с using UnityEditor.Animations возможны сбои при AddComponent.
    /// </summary>
    static UnityEngine.Animator EnsurePlayerAnimator(GameObject root)
    {
        var playerAnimator = root.GetComponent<UnityEngine.Animator>();
        if (playerAnimator != null)
            return playerAnimator;

        playerAnimator = Undo.AddComponent<UnityEngine.Animator>(root);
        if (playerAnimator != null)
            return playerAnimator;

        return root.AddComponent<UnityEngine.Animator>();
    }

    static void AssignNpcVisual(string dataPath, string prefabPath, string texturePath, int col, int row)
    {
        var sprite = LoadGridSprite(texturePath, col, row);
        if (sprite == null)
        {
            Debug.LogWarning($"ArtImportPipeline: спрайт не найден {texturePath} [{col},{row}]");
            return;
        }

        var data = AssetDatabase.LoadAssetAtPath<CharacterVisualData>(dataPath);
        if (data == null)
            return;

        data.defaultSprite = sprite;
        data.pixelsPerUnit = GameArtSettings.RoguesPixelsPerUnit;
        data.sortingLayerName = GameArtSettings.SortingLayers.Player;
        EditorUtility.SetDirty(data);

        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        var applicator = root.GetComponent<CharacterVisualApplicator>() ?? root.AddComponent<CharacterVisualApplicator>();
        var appSo = new SerializedObject(applicator);
        appSo.FindProperty("visualData").objectReferenceValue = data;
        appSo.ApplyModifiedPropertiesWithoutUndo();

        var sr = root.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprite;
            sr.color = Color.white;
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
