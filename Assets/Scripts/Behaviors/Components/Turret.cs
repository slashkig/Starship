using SpaceGame;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class Turret : GunModule<GunStats>
{
    protected override IEnumerator FireCycle()
    {
        while (true)
        {
            yield return new WaitUntil(() => Target != null && ship.Resources[stats.projectile.GetComponent<Projectile>().stats
                .ammoType] >= 1 && ship.Resources[Resource.Power] >= 0.4f && targetLock);
            Projectile shotScript = Instantiate(stats.projectile, transform.position + transform.up * 2, transform.rotation)
                .GetComponent<Projectile>();
            shotScript.originId = ship.id;
            ship.UpdateResources(-1, shotScript.stats.ammoType, false);
            ship.UpdateResources(-0.4f, Resource.Power);
            if (shotScript is Missile missile) { missile.GetComponentInChildren<AimConstraint>().SetSource(0,
                GameMethods.AimSource(ship.Target)); }
            yield return new WaitForSeconds(stats.reloadTime);
        }
    }
}