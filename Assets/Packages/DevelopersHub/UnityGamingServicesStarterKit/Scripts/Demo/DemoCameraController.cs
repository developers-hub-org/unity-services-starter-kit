using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoCameraController : MonoBehaviour
    {

        [Header("Target Settings")]
        [SerializeField] private Transform _target = null;
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        [Header("Camera Settings")]
        [SerializeField] private Camera _camera = null; public Camera Camera { get { return _camera; } }
        [SerializeField] private float distance = 5f;
        //[SerializeField] private float minDistance = 2f;
        //[SerializeField] private float maxDistance = 10f;
        //[SerializeField] private float zoomSpeed = 2f;

        [Header("Rotation Settings")]
        [SerializeField] private float xRotationSpeed = 200f;
        [SerializeField] private float yRotationSpeed = 200f;
        [SerializeField] private float minVerticalAngle = -20f;
        [SerializeField] private float maxVerticalAngle = 80f;

        [Header("Collision Settings")]
        [SerializeField] private string collisionIgnoreTag = "Player";
        [SerializeField] private float collisionOffset = 0.3f;

        private float currentX = 0f;
        private float currentY = 30f;
        private float currentD = 5f;
        private Transform cameraTransform = null;
        private Vector3 cameraPosition;

        private ClientRuntimeMenuPanel _menuPanel = null;

        private void Start()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                if (_camera == null)
                {
                    _camera = Camera.main;
                }
            }
            cameraTransform = _camera.transform;

            // Initialize camera angles
            currentX = cameraTransform.eulerAngles.y;
            currentY = cameraTransform.eulerAngles.x;
            currentD = distance;

            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _menuPanel = CanvasPanel.Get<ClientRuntimeMenuPanel>();
        }

        private void LateUpdate()
        {
            if (_target == null) { return; }
            HandleInput();
            UpdateCameraPosition();
            HandleCameraCollision();
            UpdateCameraTransform();
        }

        private void HandleInput()
        {
            // Camera rotation
            Vector2 lookInput = InputManager.Instance.Look;
            if (_menuPanel.IsVisible)
            {
                lookInput = Vector2.zero;
            }
            currentX += lookInput.x * xRotationSpeed * Time.deltaTime;
            currentY -= lookInput.y * yRotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        private void UpdateCameraPosition()
        {
            // Calculate desired camera position based on orbit angles and distance
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = _target.position + targetOffset - (rotation * Vector3.forward * currentD);
            cameraPosition = desiredPosition;
        }

        private void HandleCameraCollision()
        {
            RaycastHit hit;
            Vector3 direction = cameraPosition - (_target.position + targetOffset);
            float rayDistance = direction.magnitude;

            // Check for obstacles between camera and target
            if (Physics.Raycast(_target.position + targetOffset, direction.normalized, out hit, rayDistance))
            {
                if (!hit.collider.CompareTag(collisionIgnoreTag))
                {
                    // Move camera in front of the obstacle
                    cameraPosition = hit.point - (direction.normalized * collisionOffset);
                }
            }
        }

        private void UpdateCameraTransform()
        {
            // Update camera position and rotation
            cameraTransform.position = cameraPosition;
            cameraTransform.LookAt(_target.position + targetOffset);
        }

        public void SetTarget(DemoCameraTarget target)
        {
            _target = target != null ? target.Target : null;
            if (_target != null)
            {
                Vector3 direction = transform.position - (_target.position + targetOffset);
                currentD = direction.magnitude;
                currentX = _target.eulerAngles.y;
                currentY = transform.eulerAngles.x;
            }
        }

    }
}