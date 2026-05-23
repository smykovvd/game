using UnityEngine;

[CreateAssetMenu(fileName = "PropData", menuName = "Game/Prop Data")]
public class PropData : ScriptableObject
{
    public Sprite sprite;
    public string sortingLayerName = GameArtSettings.SortingLayers.Props;
    public int sortingOrder;
    public Vector2 colliderSize = Vector2.one;
    public bool isTrigger;
}
