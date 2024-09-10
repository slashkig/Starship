using UnityEngine;

[CreateAssetMenu(fileName = "GunStats.asset", menuName = "Object/Turret", order = 1)]
public class GunStats : ModuleStats
{
    public float reloadTime;
    public GameObject projectile;
}