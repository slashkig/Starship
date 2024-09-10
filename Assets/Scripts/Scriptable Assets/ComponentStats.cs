using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComponentStats.asset", menuName = "Object/Component", order = 0)]
public class ComponentStats : ObjectStats
{
    public List<PairList<Health, Resource>> repairCost;
    public List<EnumPair<Resource>> cost;
}