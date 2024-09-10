using Extensions.Enums;
using SpaceGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ModularComponent : ShipComponent
{
    public GameObject defaultComponent;
    [HideInInspector] public List<EnumPair<Health>> healthIncrease;
    private GameObject module;

    protected override void OnCreate()
    {
        base.OnCreate();
        if (defaultComponent != null) AddModule(defaultComponent);
    }

    public void IncreaseMaxHealth(List<EnumPair<Health>> amounts)
    {
        healthIncrease = amounts;
        foreach (EnumPair<Health> healthPair in amounts)
        {
            health[healthPair.type] += healthPair.value;
            Slider healthBar = healthSliders[(int)healthPair.type];
            healthBar.maxValue += healthPair.value;
            healthBar.value += healthPair.value;
        }
    }

    public void AddModule(GameObject component) => module = Instantiate(component, transform.position, transform.rotation, transform);

    protected override void Die()
    {
        module?.GetComponent<IDestroyable>().Die();
        base.Die();
    }
}