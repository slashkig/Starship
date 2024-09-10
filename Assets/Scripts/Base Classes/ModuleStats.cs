using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;

public abstract class ModuleStats : ObjectStats
{
    public List<EnumPair<Resource>> cost;
}