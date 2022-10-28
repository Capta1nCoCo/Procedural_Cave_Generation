using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFor2D : MonoBehaviour
{
    private Rigidbody rbody;
    private Vector3 velocity;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rbody.MovePosition(rbody.position + velocity * Time.fixedDeltaTime);
    }

    private void Update()
    {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * 10;
    }
}
