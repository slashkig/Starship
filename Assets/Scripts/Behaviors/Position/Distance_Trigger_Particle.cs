using UnityEngine;

public class Distance_Trigger_Particle : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Projectile projectile) && !(projectile is Missile)) { Destroy(projectile.gameObject); }
    }
}