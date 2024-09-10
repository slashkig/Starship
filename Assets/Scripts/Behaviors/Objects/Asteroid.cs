using SpaceGame;
using UnityEngine;
using static Extensions.Toolbox;
using static Extensions.TransformMethods;

public class Asteroid : ObjectCollider
{
    public Rigidbody2D selfRb;
    private float rotationSpeed;
    private Vector2 velocity;

    protected override void OnCreate()
    {
        base.OnCreate();
        AsteroidStats stats = (AsteroidStats)base.stats;
        selfRb.isKinematic = true;
        transform.rotation = Quaternion.Euler(0, 0, Range(180));
        rotationSpeed = Range(75);
        velocity = Random.Range(150, 350) * transform.forward;
        if (resourceValue.Count == 0)
        {
            float totalValue = selfRb.mass * Random.Range(30, 50) / 100f;
            for (int i = 0; i < stats.resourceWeights.Count; i++)
            {
                float percentage = 1f / (stats.resourceWeights.Count - i);
                float value = percentage == 1 ? totalValue : totalValue * percentage * Random.Range(0.8f, 1.2f)
                    * stats.resourceWeights[i].value;
                totalValue -= value;
                resourceValue.Add(stats.resourceWeights[i].type, RoundToTenths(value));
            }
        }
    }

    public void PausePhysics()
    {
        rotationSpeed = selfRb.angularVelocity;
        velocity = selfRb.velocity;
        selfRb.isKinematic = true;
    }

    public void ResumePhysics()
    {
        selfRb.isKinematic = false;
        selfRb.angularVelocity = rotationSpeed;
        selfRb.velocity = velocity;
    }

    protected override void Die()
    {
        for (int i = 0; i < ((AsteroidStats)stats).numFragments + Random.Range(-1, 1); i++)
        {
            Vector3 randPos = RandPos();
            GameObject fragment = Instantiate(((AsteroidStats)stats).fragment, transform.position + randPos,
                transform.rotation, GameMethods.Manager.asteroidParent);
            fragment.GetComponent<Rigidbody2D>().AddForce(randPos * 6);
            if (fragment.TryGetComponent(out Asteroid asteroid))
            {
                var fragmentValues = asteroid.resourceValue;
                foreach (Resource resource in fragmentValues.Keys) { resourceValue[resource] -= fragmentValues[resource]; }
            }
        }
        base.Die();
    }
 }