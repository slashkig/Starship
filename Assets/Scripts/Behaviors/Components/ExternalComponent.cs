public class ExternalComponent : ShipComponent
{
    ExternalSpace selfSpace;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        selfSpace = new ExternalSpace(this);
    }

    protected override void Die()
    {
        selfSpace.Die();
        base.Die();
    }
}