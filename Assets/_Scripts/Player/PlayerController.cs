using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public Camera playerCamera;
    public float mouseSensitivity = 1f;
    public float mobileSensitivity = 1f;
    public float lookSensitivity;
    public bool canMove = false;

    [Header("Feet Settings")]
    public GameObject footL;
    public GameObject footR;
    public float stepAmplitude = 0.2f;
    public float stepInterval = 0.3f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    private Vector3 footLDefault;
    private Vector3 footRDefault;
    private bool leftStepForward = true;
    private float stepTimer = 0f;

    // --- Input Actions ---
    private NewControls inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool kickPressed;
    private bool shopPressed;

    void Awake()
    {
        instance = this;
        inputActions = new NewControls();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.MoveCam.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.MoveCam.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Kick.performed += ctx => kickPressed = true;
        inputActions.Player.ShopMenu.performed += ctx => shopPressed = true;

        bool isMobile = Application.isMobilePlatform;
        lookSensitivity = isMobile? mobileSensitivity : mouseSensitivity;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (footL != null) footLDefault = footL.transform.localPosition;
        if (footR != null) footRDefault = footR.transform.localPosition;
    }

    void Update()
    {
        if(canMove) HandleMovement();
        if(!ShopManager.instance.isOpen && !ShopManager.instance.isEsc && canMove) HandleLook();
        HandleFootsteps();
        HandleActions();

    }

    private Vector3 externalVelocity;
    [Header("Hit Reaction")]
    public float knockbackDrag = 8f;
    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 horizontalMove = move * moveSpeed;
        //controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        //controller.Move(velocity * Time.deltaTime);
        controller.Move((horizontalMove + externalVelocity + new Vector3(0, velocity.y, 0)) * Time.deltaTime);

        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, knockbackDrag * Time.deltaTime);
    }
    public void ApplyKnockback(Vector3 direction, float force, float upwardForce = 0f)
    {
        direction.y = 0f;
        direction.Normalize();

        externalVelocity += direction * force;
        externalVelocity.y = upwardForce;
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleFootsteps()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                leftStepForward = !leftStepForward;
                stepTimer = 0f;
            }

            float stepOffset = stepAmplitude;
            Vector3 leftTarget, rightTarget;

            if (leftStepForward)
            {
                leftTarget = footLDefault + new Vector3(0, 0, stepOffset);
                rightTarget = footRDefault + new Vector3(0, 0, -stepOffset);
            }
            else
            {
                leftTarget = footLDefault + new Vector3(0, 0, -stepOffset);
                rightTarget = footRDefault + new Vector3(0, 0, stepOffset);
            }

            if (footL != null)
                footL.transform.localPosition = Vector3.Lerp(footL.transform.localPosition, leftTarget, Time.deltaTime * 10f);
            if (footR != null)
                footR.transform.localPosition = Vector3.Lerp(footR.transform.localPosition, rightTarget, Time.deltaTime * 10f);
        }
        else
        {
            if (footL != null)
                footL.transform.localPosition = Vector3.Lerp(footL.transform.localPosition, footLDefault, Time.deltaTime * 10f);
            if (footR != null)
                footR.transform.localPosition = Vector3.Lerp(footR.transform.localPosition, footRDefault, Time.deltaTime * 10f);

            stepTimer = 0f;
        }
    }

    void HandleActions()
    {
        if (kickPressed)
        {
            // TODO: Kick logic here
            kickPressed = false;
        }

        if (shopPressed)
        {
            // TODO: Shop menu logic here
            shopPressed = false;
        }
    }
    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;

        if (!Application.isMobilePlatform)
            lookSensitivity = value;
    }

    public void SetMobileSensitivity(float value)
    {
        mobileSensitivity = value;

        if (Application.isMobilePlatform)
            lookSensitivity = value;
    }

}
