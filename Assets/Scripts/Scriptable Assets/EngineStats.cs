using UnityEngine;

[CreateAssetMenu(menuName = "Object/Engine")]
public class EngineStats : ComponentStats
{
    public float forwardSpeed;
    public float turnSpeed;
    public ParticleSystem engineTrail;
}