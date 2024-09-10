using Extensions;
using SpaceGame;
using System.Collections;
using UnityEngine;

public class Laser : GunModule<LaserStats>
{
    public ParticleBeam laser;
    [HideInInspector] public bool firing = false;
    private float heat;

    protected override void OnCreate()
    {
        laser.damage = stats.damage;
        ParticleSystem.CollisionModule laserCollision = laser.beam.collision;
        laserCollision.collidesWith -= 1 << gameObject.layer;
    }
    
    void Update()
    {
        if (!firing && heat > 0) heat = Mathf.Max(0, heat - Time.deltaTime);
    }

    public override void Die()
    {
        StopAllCoroutines();
        laser.transform.SetParent(null);
        ParticleSystem.MainModule system = laser.beam.main;
        system.stopAction = ParticleSystemStopAction.Destroy;
        laser.beam.Stop();
        base.Die();
    }
    
    protected override IEnumerator FireCycle()
    {
        while (true)
        {
            yield return new WaitUntil(() => Target != null && transform.Distance(Target) < stats.range &&
                ship.Resources[Resource.Power] >= 0.8f && targetLock);
            yield return new WaitForSeconds(Random.Range(0, 0.1f));
            laser.beam.Play();
            firing = true;
            while (targetLock && ship.Resources[Resource.Power] > 0.8f && heat < stats.maxHeat)
            {
                ship.UpdateResources(-0.3f, Resource.Power, false);
                heat += 0.5f;
                yield return new WaitForSeconds(0.25f);
            }
            laser.beam.Stop();
            firing = false;
            if (heat > stats.maxHeat) yield return new WaitUntil(() => heat == 0);
        }
    }
}