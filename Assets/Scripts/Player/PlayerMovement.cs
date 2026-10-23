using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Animator))]
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Network Identity")]
    [HideInInspector] public int id;
    public Player photonPlayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float minPitch = -80f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;

    [Header("Animation")]
    [SerializeField] private float animationDampTime = 0.1f;

    private Rigidbody rb;
    private Animator anim;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float cameraPitch;
    private bool isGrounded;
    private bool wasGrounded;

    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int IsJumpHash = Animator.StringToHash("IsJump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        // Enforce Ownership Control Settings
        if (photonView.IsMine)
        {
            // Enable local control systems for local owner
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null)
                playerCamera.enabled = true;

            if (playerInput != null)
                playerInput.enabled = true;
        }
        else
        {
            // Disable local control & rendering systems for remote players
            if (playerCamera != null)
                playerCamera.enabled = false;

            if (playerInput != null)
                playerInput.enabled = false;

            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
                listener.enabled = false;
        }
    }

    public void InitializePlayer(Player player)
    {
        photonPlayer = player;
        if (player != null)
            id = player.ActorNumber;
    }

    private void Update()
    {
        // Guard clause: Only the local owner calculates updates
        if (!photonView.IsMine)
            return;

        wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                groundLayer
            );
        }
        else
        {
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                1.1f,
                groundLayer
            );
        }

        if (isGrounded && !wasGrounded && anim != null)
        {
            anim.SetBool(IsJumpHash, false);
        }

        HandleLook();
    }

    private void FixedUpdate()
    {
        // Guard clause: Only the local owner calculates physics
        if (!photonView.IsMine)
            return;

        HandleMovement();
    }

    #region Input System Callbacks

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine)
            return;

        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine || !context.performed || !isGrounded)
            return;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );

        if (anim != null)
            anim.SetBool(IsJumpHash, true);
    }

    #endregion

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minPitch,
            maxPitch
        );

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        if (cameraTransform == null)
            return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * moveInput.y + right * moveInput.x) * moveSpeed;

        rb.linearVelocity = new Vector3(
            direction.x,
            rb.linearVelocity.y,
            direction.z
        );

        if (anim != null)
        {
            anim.SetFloat(
                HorizontalHash,
                moveInput.x,
                animationDampTime,
                Time.fixedDeltaTime
            );

            anim.SetFloat(
                VerticalHash,
                moveInput.y,
                animationDampTime,
                Time.fixedDeltaTime
            );
        }
    }
}