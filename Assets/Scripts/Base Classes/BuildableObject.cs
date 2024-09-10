using Extensions.Enums;
using SpaceGame;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Extensions.Toolbox;
using static SpaceGame.GameMethods;

public abstract class BuildableObject : ObjectCollider
{
    [HideInInspector] public Ship ship;

    public ComponentStats Stats => (ComponentStats)stats;

    protected override void OnCreate()
    {
        base.OnCreate();
        EnumMethods.AddToDict(Stats.cost, ref resourceValue);
        ship = GetComponentInParent<Ship>();
        id = ship.playerNumber;
    }

    public void AddResourceValue(Dictionary<Enum, float> resources)
    {
        foreach (KeyValuePair<Enum, float> resource in resources)
        {
            if (resourceValue.ContainsKey(resource.Key)) resourceValue[resource.Key] += resource.Value;
            else resourceValue.Add(resource.Key, resource.Value);
        }
    }

    public void Repair(Health type, float value)
    {
        float factor = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 10 : 1;
        float healthMissing = Stats.startingHealth[(int)type].value + (this is ModularComponent component ?
            component.healthIncrease[(int)type].value : 0) - health[type];

        if (!Stats.repairCost[(int)type].pairs.TrueForAll((EnumPair<Resource> resource) => ship.Resources[resource.type] >=
            resource.value * factor) || healthMissing <= 0) { return; }
        else if (healthMissing < factor) { factor = RoundToTenths(healthMissing); }
        
        health[type] += value * factor;
        healthSliders[(int)type].value = health[type];
        foreach (EnumPair<Resource> resource in Stats.repairCost[(int)type].pairs)
            { ship.UpdateResources(-resource.value * factor, resource.type, false); }
        ship.UpdateResources(-0.1f * factor, Resource.Power);
        if (Displayed) Display();
        if (Stats.startingHealth.TrueForAll((EnumPair<Health> maxHealth) => maxHealth.value == health[maxHealth.type]))
            { healthBar.SetActive(false); }
    }

    protected override void Die()
    {
        if (this == Manager.repairingObject) Manager.DisplayRepair();
        ship.OnShipDeath -= Die;
        base.Die();
    }
}