using Extensions;
using SpaceGame;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Extensions.Toolbox;
using static Extensions.TransformMethods;
using static SpaceGame.GameMethods;

/// <summary> The base class for all destroyable objects in the game. </summary>
public abstract class SpaceGameObject : MonoBehaviour
{
    protected bool Displayed { get => Manager.displayedObject == this; set => Manager.displayedObject = value ? this : null; }

    public Dictionary<Enum, float> resourceValue = new Dictionary<Enum, float>();
    protected Collider2D self;

    protected abstract void OnCreate();

    void Awake()
    {
        self = TryGetComponent(out Collider2D collider) ? collider : null;
        OnCreate();
    }

    void OnMouseEnter() => Display();

    void OnMouseExit()
    {
        if (Displayed) Display(false);
    }

    protected virtual Vector3 RandPos() => transform.RandPos(self);

    protected virtual void Die()
    {
        if (Displayed) Display(false);
        float value;
        foreach (Enum type in resourceValue.Keys)
        {
            value = resourceValue[type];
            while (value > 0)
            {
                Vector3 randPos = RandPos();
                Pickup piece = (Pickup)ObjManager.resourcePooler.Pool(transform.position + randPos, transform.rotation);
                piece.selfRb.AddForce(randPos * 4);
                piece.type = type;
                value -= piece.value = value <= 5 ? value : type is Ammo ? UnityEngine.Random.Range(3, 5) : 4 + Range(10) / 10;
            }
        }
        StopAllCoroutines();
        Destroy(gameObject);
    }

    public abstract void Display(bool show = true);
}
