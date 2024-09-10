using Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using static SpaceGame.GameMethods;

public abstract class GunModule<T> : ComponentModule<T, Emplacement> where T : ModuleStats
{
    protected AimConstraint aim;
    protected bool targetLock = false;

    public Transform Target { get => ship.Target; private set { aim.SetSource(0, AimSource(value)); targetLock = false; } }

    void Start()
    {
        aim = Instantiate(ObjManager.targeter, transform).GetComponent<AimConstraint>();
        for (int i = 0; i < transform.childCount; i++) { transform.GetChild(i).tag = tag; }
        OnCreate();
        ship.OnTargetChanged += ChangeTarget;
        StartCoroutine(FireCycle());
    }

    void FixedUpdate()
    {
        if (Target != null && ship.powered)
        {
            transform.MatchRotation(aim.transform, 1);
            if (!targetLock && transform.AngularDifference(aim.transform) < 5) { targetLock = true; }
        }
        else if (targetLock)
        {
            targetLock = false;
        }
    }

    protected void ChangeTarget() => Target = ship.Target;

    protected virtual void OnCreate() { return; }

    public override void Die() => ship.OnTargetChanged -= ChangeTarget;

    protected abstract IEnumerator FireCycle();
}