using UnityEngine;

public class Distance_Trigger_Asteroid : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Asteroid asteroid)) { asteroid.ResumePhysics(); }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Asteroid asteroid)) { asteroid.PausePhysics(); }
    }
}