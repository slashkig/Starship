using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    private Rigidbody2D attachedRb;

    void Start() => attachedRb = GetComponentInParent<Rigidbody2D>();

    void Update()
    {
        attachedRb.AddTorque(Input.GetAxis("Horizontal"));
        attachedRb.AddForce(transform.parent.up * Input.GetAxis("Vertical") * 20);
    }
}