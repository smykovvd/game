# Графика и анимации — что уже в проекте и что сделать в Unity

## Уже сделано в репозитории

- Папки `Assets/Graphics/`, `Assets/Data/`, шаблонный `Character_Base.prefab`
- Спрайты: `Graphics/Characters/Player/Sprites/Player.png`, `Graphics/UI/Sprites/circle.png` (маска видимости)
- ScriptableObject: `Data/Characters/*VisualData.asset`
- Скрипты: `CharacterVisualApplicator`, `CharacterAnimationBridge`, `PropVisualApplicator`, `WorldDialogBubble`, `MainMenuController`
- Sorting Layers: добавлены **Props** и **Characters** (старые слои сохранены)
- Меню Unity: **Game → Setup → …** (см. ниже)

---

## Импорт ваших паков (RPG_Hero, 32rogues, P_P_FREE_RPG_TILESET)

Файлы лежат в `Assets/Graphics/_Source/` и `Assets/Graphics/Characters/Player/Sprites/RPG_Hero/`.

1. Откройте Unity, дождитесь компиляции.
2. **Game → Import → Import All Art Packs** — нарезка спрайтов, анимации игрока, Animator, тайлы, спрайты NPC.
3. **Window → 2D → Tile Palette** → Create New Palette → папка `Assets/Graphics/Environment/Tiles/Dungeon` (или Island / 32rogues).
4. Откройте `LevelTemplate` и рисуйте тайлами.

Частичный импорт: **Hero Only** / **Tilesets Only** в том же меню.

| Пак | PPU | Назначение |
|-----|-----|------------|
| RPG_Hero | 40 | Игрок, 4 направления idle/run/attack |
| 32rogues | 32 | Wolf = orc (0,0), Guard = bandit (3,0) на листах |
| P_P tileset | 24 | Dungeon / Island тайлмап |

---

## Первый запуск в Unity (5–10 минут)

1. Откройте проект в Unity 6, дождитесь компиляции.
2. Выполните по порядку:
   - **Game → Setup → 1. Create Default Visual Data Assets** (если ассеты уже есть — пропустится)
   - **Game → Setup → 2. Create Level Template Scene**
   - **Game → Setup → 3. Create Main Menu Scene**
   - **Game → Setup → 4. Add Visual Components To Player Prefab**
3. **File → Build Settings** — добавьте сцены `MainMenu` и игровые уровни.
4. Откройте `Player.prefab` — проверьте, что на корне есть `CharacterVisualApplicator` (поле **Visual Data** → `PlayerVisualData`) и `CharacterAnimationBridge`.

---

## Часть A. Художник: спрайты и анимации

### A1. Импорт PNG

1. Положите файл в нужную папку, например `Graphics/Characters/Player/Sprites/`.
2. Выберите текстуру в Project:
   - **Texture Type**: Sprite (2D and UI)
   - **Sprite Mode**: Multiple (лист) или Single
   - **Pixels Per Unit**: **256** сейчас у placeholder; для нового арта договоритесь в команде (16 / 32 / 64)
   - **Filter Mode**: Point (no filter)
   - **Compression**: None (для пиксель-арта)
3. **Sprite Editor** → Slice → Apply.

### A2. Animation Clips

1. Выделите кадры в Project.
2. ПКМ → **Create → Animation**.
3. Сохраните в `Graphics/Characters/Player/Animations/`, имена: `Player_Idle_Down`, `Player_Walk_Left`, …
4. В окне **Animation** задайте Samples (8–12 FPS для pixel art).

### A3. Animator Controller

1. ПКМ в `Graphics/Characters/Player/Animators/` → **Create → Animator Controller** → `Player.controller`.
2. Вкладка **Animator**, параметры (типы важны):

| Имя | Тип |
|-----|-----|
| Speed | Float |
| MoveX | Float |
| MoveY | Float |
| IsAttacking | Bool |
| AttackTrigger | Trigger |

3. Состояния: Idle, Walk (Blend Tree 2D Simple Directional по MoveX/MoveY), Attack.
4. Переходы:
   - Idle ↔ Walk: `Speed > 0.01` / `Speed < 0.01`
   - Any State → Attack: `AttackTrigger`
   - Attack → Idle: Exit Time ~0.9

5. Откройте `Data/Characters/PlayerVisualData.asset` → перетащите **Animator Controller** в поле **Animator Controller**.

### A4. Префаб с Visual (рекомендуется для новых персонажей)

1. Дублируйте `Prefabs/Characters/Character_Base.prefab` → назовите, например, `Hero.prefab`.
2. На **Visual** назначьте спрайт по умолчанию.
3. Создайте **Prefab Variant** от `Character_Base` или назначьте свой `CharacterVisualData` на `CharacterVisualApplicator`.

Старый `Player.prefab` пока с SpriteRenderer на корне — работает. После появления анимаций можно перенести рендер на дочерний **Visual** (см. часть E).

---

## Часть B. NPC

1. Анимации и контроллер — в `Graphics/Characters/NPC/Wolf/` (или Guard).
2. Заполните `WolfVisualData.asset` / `GuardVisualData.asset`.
3. На префабе `WolfEnemy` / `PatrollingGuard` добавьте:
   - `CharacterVisualApplicator` + свой Visual Data
   - `CharacterAnimationBridge` (если есть Animator)
4. Точки патруля: пустые объекты в сцене → перетащите в список **Patrol Points** у `PatrollingGuard`.

---

## Часть C. Tilemap (редактор уровней)

### C1. Пакет (если нет Rule Tile)

**Window → Package Manager** → Unity Registry → **2D Tilemap Extras** → Install.

### C2. Тайлсет (художник)

1. PNG в `Graphics/Environment/Tilesets/`.
2. Slice в Sprite Editor (размер клетки = ваш тайл, например 16×16).
3. Выделите спрайты → **Create → 2D → Tiles → Tile** (или Rule Tile для стен).
4. **Window → 2D → Tile Palette** → Create New Palette → сохраните в `Graphics/Environment/Palettes/`.
5. Перетащите тайлы в палитру.

### C3. Уровень (редактор)

1. **File → New Scene** или дублируйте `Scenes/_Templates/LevelTemplate.unity`.
2. Откройте Palette, выберите слой **Tilemap_Ground** — рисуйте пол.
3. **Tilemap_Collision** — только непроходимые тайлы (или отдельный «collision»-тайл).
4. **Tilemap_Decoration** — декор.
5. Объекты (камни, NPC) — из `Prefabs/` в пустой **Objects**.
6. **PlayerSpawn** — пустой Transform, откуда спавнить игрока (логику спавна добавит программист).

### C4. Сетка

- В шаблоне **Cell Size = 1** (`GameArtSettings.GridCellSize`).
- PPU тайла должен давать ровно 1 unit на клетку: тайл 16px при PPU 16 → 1 unit.

---

## Часть D. UI

### D1. Главное меню

1. Откройте `Scenes/MainMenu.unity` (создаётся пунктом меню Game).
2. На Canvas добавьте Panel + кнопки (можно дублировать `Prefabs/UI/Button.prefab`).
3. **On Click ()** кнопки Play → объект `MainMenuController` → **MainMenuController.Play**.
4. Quit → **MainMenuController.Quit**.
5. Текст — только **TextMeshProUGUI**, не legacy Text.

### D2. Речь над персонажем

1. **GameObject → UI → Canvas**, Render Mode: **World Space**, Scale ~0.01.
2. Дочерний Panel + **TextMeshProUGUI**.
3. Добавьте скрипт `WorldDialogBubble`, укажите **Message Text** и **Follow Target** = `DialogAnchor` на персонаже.
4. Сохраните как `Prefabs/UI/WorldDialogBubble.prefab`.
5. Из кода: `bubble.Show("Привет!", npcDialogAnchor);`

### D3. Выборы (уже есть)

Используйте `Prefabs/UI/ChoicePanel.prefab` + `ChoiceManager` — только Screen Space UI.

---

## Часть E. Миграция Player на структуру Visual (опционально)

1. Откройте `Player.prefab`.
2. Создайте дочерний объект **Visual**, перенесите на него **SpriteRenderer** и добавьте **Animator**.
3. На корне оставьте Rigidbody2D, коллайдеры, скрипты движения.
4. `CharacterVisualApplicator` → **Visual Root** = Transform Visual.
5. `CharacterAnimationBridge` → **Animator** и **Flip Target** = Visual.
6. **VisibilityMask** оставьте дочерним корня (как сейчас).

---

## PPU и масштаб

Сейчас в коде `GameArtSettings.DefaultPixelsPerUnit = 256` под placeholder-квадрат. Перед массовым импортом арта **зафиксируйте одно значение** и обновите:

- Import Settings всех спрайтов
- поле **Pixels Per Unit** в `*VisualData.asset`
- константу в `GameArtSettings.cs` (при необходимости)

---

## Структура папок (шпаргалка)

```
Graphics/     — PNG, .anim, .controller
Data/         — ScriptableObject (.asset)
Prefabs/      — готовые объекты для сцен
Scenes/       — уровни и _Templates/
Scripts/      — логика (Data, Visual, UI, Editor)
```

---

## Меню Game → Setup

| Пункт | Действие |
|-------|----------|
| 1 | Создаёт CharacterVisualData, если нет |
| 2 | Сцена с Grid + 3 Tilemap + Objects/NPC/PlayerSpawn |
| 3 | Пустая MainMenu + MainMenuController |
| 4 | Добавляет компоненты на Player.prefab |

При проблемах с GUID после ручных правок: выберите скрипт в Inspector → Reimport или перепривяжите Missing Script.
