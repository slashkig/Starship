using UnityEngine;

[CreateAssetMenu(fileName = "LaserStats.asset", menuName = "Object/Laser", order = 2)]
public class LaserStats : ModuleStats
{
    public float damage;
    public float maxHeat;
    public int range;
}