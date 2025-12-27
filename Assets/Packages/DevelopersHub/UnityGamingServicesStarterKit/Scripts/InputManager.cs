using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class InputManager : MonoBehaviour
    {

        private static InputManager _instance = null; public static InputManager Instance { get { GetInstance(); return _instance; } }
        private GamingServicesInputActions _inputActions = null; public GamingServicesInputActions InputActions { get { return _inputActions; } }
        private bool _enabled = false;
        private bool _jump = false; public bool Jump { get { return _jump; } }
        private bool _cancel = false; public bool Cancel { get { return _cancel; } }
        private bool _sprint = false; public bool Sprint { get { return _sprint; } }
        private Vector2 _move = Vector2.zero; public Vector2 Move { get { return _move; } }
        private Vector2 _look = Vector2.zero; public Vector2 Look { get { return _look; } }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _instance.Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<InputManager>();
            if (_instance == null)
            {
                _instance = new GameObject("InputManager").AddComponent<InputManager>();
            }
            DontDestroyOnLoad(_instance.gameObject);
            _instance.Initialize();
        }

        private void Initialize()
        {
            
        }

        private void Start()
        {
            _inputActions = new GamingServicesInputActions();
            _inputActions.Enable();
            _enabled = true;
        }

        private void OnEnable()
        {
            if (!_enabled && _inputActions != null)
            {
                _inputActions.Enable();
                _enabled = true;
            }
        }

        private void OnDisable()
        {
            if (_enabled && _inputActions != null)
            {
                _inputActions.Disable();
                _enabled = false;
            }
        }

        private void Update()
        {
            _jump = _inputActions.Player.Jump.WasPressedThisFrame();
            _cancel = _inputActions.UI.Cancel.WasPressedThisFrame();
            _sprint = _inputActions.Player.Sprint.IsPressed();
            _move = _inputActions.Player.Move.ReadValue<Vector2>();
            _look = _inputActions.Player.Look.ReadValue<Vector2>();
        }

    }
}