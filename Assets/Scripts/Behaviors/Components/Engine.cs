using UnityEngine;

public class Engine : ExternalComponent
{
    private ParticleSystem trail;

    public new EngineStats Stats => (EngineStats)stats;

    protected override void OnCreate()
    {
        base.OnCreate();
        trail = Instantiate(Stats.engineTrail, transform.position, Stats.engineTrail.transform.rotation, transform);
        trail.transform.localPosition = new Vector3(0, -0.3f, 0);
        SetTrail(false);
        ship.SetMoving += SetTrail;
        ship.forwardSpeed += Stats.forwardSpeed;
        ship.turnSpeed += Stats.turnSpeed;
        ship.numEngines++;
    }

    protected override void Die()
    {
        ship.SetMoving -= SetTrail;
        ship.forwardSpeed -= Stats.forwardSpeed;
        ship.turnSpeed -= Stats.turnSpeed;
        ship.numEngines--;
        if (trail.particleCount > 0)
        {
            trail.transform.SetParent(null);
            ParticleSystem.MainModule system = trail.main;
            system.stopAction = ParticleSystemStopAction.Destroy;
            SetTrail(false);
        }
        else { Destroy(trail); }
        base.Die();
    }

    private void SetTrail(bool setting)
    {
        if (setting) trail.Play();
        else trail.Stop();
    }
}