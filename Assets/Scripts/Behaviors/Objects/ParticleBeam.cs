using SpaceGame;
using UnityEngine;

public class ParticleBeam : MonoBehaviour
{
    public Damage type;
    public ParticleSystem beam;
    [HideInInspector] public float damage;
}