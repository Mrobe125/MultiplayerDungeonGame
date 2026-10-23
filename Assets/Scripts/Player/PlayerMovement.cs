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

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 5.0f;

    [Header("Camera & Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxPitch = 80.0f;
    [SerializeField] private float minPitch = -80.0f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;

    [Header("Animation Settings")]
    [SerializeField] private float animationDampTime = 0.1f;

    private Animator _anim;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0.0f;
    private bool isGrounded;
    private bool wasGrounded;

    // Hashes for optimal parameter updates
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int IsJumpHash = Animator.StringToHash("IsJump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null) playerCamera.enabled = true;
        }
        else
        {
            if (playerCamera != null) playerCamera.enabled = false;
            rb.isKinematic = true;
        }
    }

    [PunRPC]
    public void InitializePlayer(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // Ground Check
        wasGrounded = isGrounded;
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        }

        if (isGrounded && !wasGrounded && _anim != null)
        {
            _anim.SetBool(IsJumpHash, false);
        }

        HandleLook();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        HandleMovement();
    }

    #region Input System Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;

        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Broadcast jump state to all clients
            photonView.RPC(nameof(RPC_SetJump), RpcTarget.All, true);
        }
    }

    [PunRPC]
    private void RPC_SetJump(bool jumping)
    {
        if (_anim != null)
        {
            _anim.SetBool(IsJumpHash, jumping);
        }
    }
    #endregion

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x) * moveSpeed;
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);

        // --- Update Blend Tree Animator Parameters ---
        if (_anim != null)
        {
            _anim.SetFloat(HorizontalHash, moveInput.x, animationDampTime, Time.fixedDeltaTime);
            _anim.SetFloat(VerticalHash, moveInput.y, animationDampTime, Time.fixedDeltaTime);
        }
    }
}