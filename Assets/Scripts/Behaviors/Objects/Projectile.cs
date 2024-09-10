using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileStats stats;
    public int originId;
    public Rigidbody2D selfRb;

    void Start()
    {
        selfRb = GetComponent<Rigidbody2D>();
        selfRb.AddForce(transform.up * stats.speed, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.TryGetComponent(out ObjectCollider hit) && originId != hit.id)
        {
            if (hit is Ship ship) { ship.GetSpaceOnEdge(transform.position).TakeDamage(stats.damage, stats.damageType); }
            else { hit.TakeDamage(stats.damage, stats.damageType); }

            OnHit();
        }
    }

    public virtual void OnHit() => Destroy(gameObject);
}