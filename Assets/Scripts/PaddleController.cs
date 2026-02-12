using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset actions;
    public string actionMapName = "Gameplay";
    public string moveActionName = "P1Move";

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float yLimit = 4f;

    Rigidbody rb;
    InputAction moveAction;

    [Header("Camera Shake")]
    public float baseShakeDuration = 0.2f;
    public float maxShakeStrength = 0.6f;

    public PerlinCameraShake camShake;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        var map = actions.FindActionMap(actionMapName, true);
        moveAction = map.FindAction(moveActionName, true);
        moveAction.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
    }

    void Start()
    {
        camShake = Camera.main.GetComponent<PerlinCameraShake>();
        if (camShake == null)
            Debug.LogError("No PerlinCameraShake found on Camera.main. Add PerlinCameraShake to your Camera or CameraRig.");
    }

    void FixedUpdate()
    {
        float input = moveAction.ReadValue<float>();

        rb.linearVelocity = new Vector3(0f, input * moveSpeed, 0f);

        var p = transform.position;
        p.y = Mathf.Clamp(p.y, -yLimit, yLimit);
        p.z = 0f;
        transform.position = p;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (camShake == null) return;

        float impactForce = collision.relativeVelocity.magnitude;
        float strength = Mathf.Clamp(impactForce * 0.1f, 0.1f, maxShakeStrength);

        camShake.Shake(baseShakeDuration, strength);
    }
}
