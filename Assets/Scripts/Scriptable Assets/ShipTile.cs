using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Ship Tile")]
public class ShipTile : Tile
{
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        Ship ship = tilemap.GetComponent<Ship>();
        if (!ship?.spaces.ContainsKey((Vector2Int)position) ?? false) new ShipSpace(this, ship, (Vector2Int)position);
        
        return base.StartUp(position, tilemap, go);
    }
}