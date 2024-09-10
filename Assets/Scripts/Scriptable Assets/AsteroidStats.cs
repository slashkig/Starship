using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidStats.asset", menuName = "Object/Asteroid", order = 3)]
public class AsteroidStats : ObjectStats
{
    public List<EnumPair<Resource>> resourceWeights;
    public GameObject fragment;
    public int numFragments;
}