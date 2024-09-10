using Extensions;
using System.Collections;
using UnityEngine;
using static Extensions.Toolbox;

public abstract class ShipComponent : BuildableObject
{
    protected override void OnCreate()
    {
        base.OnCreate();
        ship.OnShipDeath += Die;
        if (self is null) StartCoroutine(AddToShip());
    }

    protected override Vector3 RandPos() => self is null ? transform.RandPos(3, 3) : transform.RandPos(self);

    IEnumerator AddToShip()
    {
        yield return new WaitForEndOfFrame();
        ship.GetSpace(transform.position).damageHierarchy.Insert(0, this);
    }
}