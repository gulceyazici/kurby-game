using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "LabeledTile", menuName = "Tiles/LabeledTile")]
public class LabeledTile : Tile
{
    public string label;
}
