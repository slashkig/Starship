using SpaceGame;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats.asset", menuName = "Object/Projectile", order = 4)]
public class ProjectileStats : ScriptableObject
{
    public float speed;
    public float damage;
    public Ammo ammoType;
    public Damage damageType;
}