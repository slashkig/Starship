using UnityEngine;

public class Missile : Projectile
{
    void Update()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, transform.GetChild(0).rotation, 0.5f);
        selfRb.AddForce(transform.up * stats.speed * Time.deltaTime);
    }

    public override void OnHit()
    {
        ParticleSystem system = GetComponentInChildren<ParticleSystem>();
        if (!(system is null))
        {
            Transform trail = system.transform;
            trail.SetParent(null, true);
            trail.localScale = new Vector3(0.5f, 0.5f, 1);
            system.Stop();
        }
        Destroy(gameObject);
    }
}