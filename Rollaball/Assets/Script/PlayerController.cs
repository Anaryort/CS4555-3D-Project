using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum ControlGroup { P1, P2 }

    [Header("Which keyboard group controls this character?")]
    public ControlGroup controlGroup = ControlGroup.P1;

    [Tooltip("If true, will auto-select P2 when this object is tagged 'Player2' or its name contains '2'.")]
    public bool autoDetectGroup = true;

    [Header("Movement")]
    public float speed = 6f;
    public float rotationSpeed = 10f; // 0 to disable facing movement dir

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask = ~0;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    private float movementX, movementY;
    private bool jumpQueued;

    void Awake() => rb = GetComponent<Rigidbody>();

    void OnEnable()
    {
        // auto-detect P1 and P2
        if (autoDetectGroup)
        {
            if (name.Contains("2") || tag == "Player2")      controlGroup = ControlGroup.P2;
            else if (name.Contains("1") || tag == "Player1") controlGroup = ControlGroup.P1;
        }

        // Build per-player input actions
        moveAction = new InputAction("Move", InputActionType.Value);
        var comp = moveAction.AddCompositeBinding("2DVector");

        if (controlGroup == ControlGroup.P1)
        {
            comp.With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }
        else
        {
            comp.With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/rightCtrl");
        }

        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    void Update()
    {
        Vector2 dir = moveAction.ReadValue<Vector2>();
        movementX = dir.x;
        movementY = dir.y;

        if (jumpAction.WasPressedThisFrame())
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        // Move on XZ plane
        var v = rb.velocity;
        v.x = movementX * speed;
        v.z = movementY * speed;
        rb.velocity = v;

        // Face movement direction (optional)
        Vector3 look = new Vector3(movementX, 0f, movementY);
        if (rotationSpeed > 0f && look.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(look);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        }

        // Jump
        if (jumpQueued && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
        jumpQueued = false;
    }

    bool IsGrounded()
    {
        if (!groundCheck) return true;
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
