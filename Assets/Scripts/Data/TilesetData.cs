using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TilesetData", menuName = "Game/Tileset Data")]
public class TilesetData : ScriptableObject
{
    [Header("Идентификация")]
    public string biomeId;
    public string displayName;

    [Header("Палитра (назначить в Unity)")]
    [Tooltip("Prefab палитры: Window → 2D → Tile Palette → Create New Palette")]
    public GameObject tilePalettePrefab;

    [Header("Тайлы коллизии")]
    public TileBase collisionTile;

    [Header("Сетка")]
    public float cellSize = GameArtSettings.GridCellSize;
    public int pixelsPerUnit = GameArtSettings.DefaultPixelsPerUnit;
}
