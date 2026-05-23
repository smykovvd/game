/// <summary>
/// Общие константы арт-пайплайна. Меняйте PPU здесь и в импорте спрайтов.
/// </summary>
public static class GameArtSettings
{
    public const int HeroPixelsPerUnit = 40;
    public const int RoguesPixelsPerUnit = 32;
    public const int TilesetPixelsPerUnit = 24;

    public const int DefaultPixelsPerUnit = HeroPixelsPerUnit;
    public const float GridCellSize = 1f;

    public static class SortingLayers
    {
        public const string Background = "Background";
        public const string Ground = "Ground";
        public const string Props = "Props";
        public const string Objects = "Objects";
        public const string Characters = "Characters";
        public const string Player = "Player";
        public const string Effects = "Effects";
        public const string UI = "UI";
    }

    public static class AnimatorParameters
    {
        public const string Speed = "Speed";
        public const string MoveX = "MoveX";
        public const string MoveY = "MoveY";
        public const string IsAttacking = "IsAttacking";
        public const string AttackTrigger = "AttackTrigger";
    }
}
