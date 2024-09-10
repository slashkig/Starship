using SpaceGame;
using System.Collections.Generic;
using UnityEngine;
using static SpaceGame.GameMethods;

public class ShipSpace
{
    public ShipTile tile;
    public List<BuildableObject> damageHierarchy;
    public Vector2Int coords;
    protected Ship ship;

    public virtual void TakeDamage(float damage, Damage type)
    {
        if (damageHierarchy[0].TakeDamage(damage, type)) damageHierarchy.RemoveAt(0);
    }

    public virtual void Display(bool show)
    {
        if (!show) Manager.DisplayStats();
        else Manager.DisplayStats(damageHierarchy[0], show);
    }

    public ShipSpace(ShipTile tile, Ship ship, Vector2Int position) : this(ship, position)
    {
        this.tile = tile;
        damageHierarchy = new List<BuildableObject>() { ship };
    }

    protected ShipSpace(Ship ship, Vector2Int position)
    {
        this.ship = ship;
        coords = position;
        ship.spaces.Add(coords, this);
    }
}