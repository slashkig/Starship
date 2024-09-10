using Extensions;
using UnityEngine;
using UnityEngine.Animations;
using static SpaceGame.GameMethods;

public class EnemyEngine : Engine
{
    [SerializeField] Transform homeBase;
    private Rigidbody2D shipRb;
    private AimConstraint targeter;
    
    protected const int speedFactor = 10000;

    protected override void OnCreate()
    {
        base.OnCreate();
        shipRb = ship.GetComponent<Rigidbody2D>();
        targeter = Instantiate(ObjManager.targeter, transform).GetComponent<AimConstraint>();
        ship.OnTargetChanged += ChangeTarget;
    }

    void Update()
    {
        if (ship.powered && ship.Target != null || transform.Distance(homeBase) > 30)
        {
            ship.transform.MatchRotation(targeter.transform, Stats.turnSpeed * Time.deltaTime);
            shipRb.AddForce(speedFactor * Stats.forwardSpeed * Time.deltaTime * transform.up);
        }
        else if (ship.IsMoving) { ship.SetMoving(false); }
    }

    protected override void Die()
    {
        ship.OnTargetChanged -= ChangeTarget;
        base.Die();
    }

    private void ChangeTarget() => targeter.SetSource(0, AimSource(ship.Target != null ? ship.Target : homeBase));
}