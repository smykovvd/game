# game

2D Unity 6 (URP) проект.

## Быстрый старт (графика)

1. Откройте проект в Unity.
2. **Game → Import → Import All Art Packs** — RPG_Hero, 32rogues, тайлсет.
3. **Game → Setup** — пункты 1–5 (шаблон уровня, префабы).
4. Tile Palette: папка `Assets/Graphics/Environment/Tiles/Dungeon`.
5. Подробности: [Assets/Docs/GRAPHICS_SETUP_RU.md](Assets/Docs/GRAPHICS_SETUP_RU.md)

## Структура

- `Assets/Graphics/` — спрайты, анимации, контроллеры
- `Assets/Data/` — ScriptableObject (CharacterVisualData и др.)
- `Assets/Prefabs/` — префабы персонажей, UI, окружения
- `Assets/Scripts/` — код (`Data`, `Visual`, `UI`, `Editor`)
