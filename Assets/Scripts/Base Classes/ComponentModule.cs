using SpaceGame;
using UnityEngine;
using static Extensions.Enums.EnumMethods;

public abstract class ComponentModule<StatType, Component> : MonoBehaviour, IDestroyable where StatType : ModuleStats where Component : ModularComponent
{
    public StatType stats;
    protected Ship ship;

    void Awake()
    {
        Component component = transform.parent.GetComponent<Component>();
        ship = component.ship;
        component.IncreaseMaxHealth(stats.startingHealth);
        AddToDict(stats.cost, ref component.resourceValue);
        tag = component.tag;
        gameObject.layer = component.gameObject.layer;
    }

    public abstract void Die();
}