using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisualData", menuName = "Game/Character Visual Data")]
public class CharacterVisualData : ScriptableObject
{
    [Header("Анимация")]
    [Tooltip("Animator Controller с параметрами Speed, MoveX, MoveY, IsAttacking, AttackTrigger")]
    public RuntimeAnimatorController animatorController;

    [Header("Отображение")]
    [Tooltip("Статичный спрайт, если нет аниматора")]
    public Sprite defaultSprite;
    public Sprite portraitIcon;
    public string sortingLayerName = GameArtSettings.SortingLayers.Player;
    public int sortingOrder;
    public float visualScale = 1f;
    public Color tint = Color.white;

    [Header("Импорт")]
    [Tooltip("Должен совпадать с PPU в Import Settings спрайтов")]
    public int pixelsPerUnit = GameArtSettings.DefaultPixelsPerUnit;
}
