using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
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
    //[SerializeField] private AudioListener audioListener;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxPitch = 80.0f;
    [SerializeField] private float minPitch = -80.0f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0.0f;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null) playerCamera.enabled = true;
            //if (audioListener != null) audioListener.enabled = true;
        }
        else
        {
            // Disable remote camera and audio listener to prevent overriding local player view
            if (playerCamera != null) playerCamera.enabled = false;
            //if (audioListener != null) audioListener.enabled = false;

            // Disable physics simulation for remote network avatars
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
        // Only run movement and camera inputs for the local player instance
        if (!photonView.IsMine) return;

        // Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
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
    }
}
