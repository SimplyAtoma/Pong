using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset actions;
    public string actionMapName = "Gameplay";

    [Tooltip("Set to P1Move or P2Move per paddle")]
    public string moveActionName = "P1Move";

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float yLimit = 4f;

    private Rigidbody rb;
    private InputAction moveAction;

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

    void FixedUpdate()
    {
        float input = moveAction.ReadValue<float>();

        rb.linearVelocity = new Vector3(0f, input * moveSpeed, 0f);

        var p = transform.position;
        p.y = Mathf.Clamp(p.y, -yLimit, yLimit);
        p.z = 0f;
        transform.position = p;
    }
}
