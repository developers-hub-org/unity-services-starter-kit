using Unity.Netcode;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoPlayerController : NetworkBehaviour
    {

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;

        private Transform cameraTransform = null;
        private CharacterController controller = null;
        private float currentSpeed = 0;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            currentSpeed = moveSpeed;
            if (IsOwner)
            {
                DemoGameManager.Instance.LocalPlayerInstantiated(this);
            }
        }

        private void Update()
        {
            if(!IsOwner) { return; }
            HandleMovement();
        }

        private void HandleMovement()
        {
            // Get input
            Vector2 moveInput = InputManager.Instance.Move;
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            // Sprint input
            if (InputManager.Instance.Sprint)
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = moveSpeed;
            }

            // Create movement direction relative to camera
            Vector3 moveDirection = Vector3.zero;
            if (cameraTransform != null)
            {
                // Get camera forward and right vectors (ignoring Y rotation for movement)
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;

                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // Calculate movement direction based on camera orientation
                moveDirection = (cameraForward * vertical) + (cameraRight * horizontal);
            }
            else
            {
                // Fallback to world space if no camera
                moveDirection = new Vector3(horizontal, 0, vertical);
            }

            // Normalize movement vector to prevent faster diagonal movement
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // Move the character
            if (moveDirection.magnitude > 0.1f)
            {
                // Calculate target rotation based on movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                // Smoothly rotate towards movement direction
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Move character
                controller.Move(moveDirection * currentSpeed * Time.deltaTime);
            }
        }

        public void SetCameraTransform(Transform newCameraTransform)
        {
            cameraTransform = newCameraTransform;
        }

    }
}