using UnityEngine;

public class FaceVelocity : MonoBehaviour
{
    private Rigidbody2D targetRb;

    void Start() => targetRb = GetComponentInParent<Rigidbody2D>();

    void FixedUpdate() => transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.zero, targetRb.velocity),
        Vector3.forward);
}
