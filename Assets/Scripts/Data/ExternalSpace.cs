using SpaceGame;
using UnityEngine;

public class ExternalSpace : ShipSpace
{
    public ShipComponent component;

    public override void TakeDamage(float damage, Damage type) => component.TakeDamage(damage, type);

    public void Die()
    {
        ship.tilemap.SetTile((Vector3Int)coords, null);
        ship.spaces.Remove(coords);
    }

    public override void Display(bool show) => component.Display(show);

    public ExternalSpace(ShipComponent component) : base(component.ship,(Vector2Int)component.ship.tilemap.
        WorldToCell(component.transform.position)) { this.component = component; }
}