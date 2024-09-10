using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectStats : ScriptableObject
{
    public List<EnumPair<Health>> startingHealth = new List<EnumPair<Health>>(3);
}