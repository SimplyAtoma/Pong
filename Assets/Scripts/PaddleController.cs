using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    public float speed = 0;
    private Rigidbody rb;
    private float movementY = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    public void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementY = movementVector.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(0.0f, movementY, 0.0f);
        rb.AddForce(movement * speed);
    }
}
