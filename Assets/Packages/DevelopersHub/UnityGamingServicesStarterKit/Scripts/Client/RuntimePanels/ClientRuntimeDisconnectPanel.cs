using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeDisconnectPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonQuit = null;

        private CursorLockMode _cursorLockMode = CursorLockMode.None;
        private bool _cursorVisible = true;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonQuit.onClick.AddListener(Quit);
        }

        public override void Show()
        {
            if (ClientTerminal.Instance.Completed)
            {
                return;
            }
            base.Show();
            _cursorLockMode = Cursor.lockState;
            _cursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void Hide()
        {
            base.Hide();
            Cursor.lockState = _cursorLockMode;
            Cursor.visible = _cursorVisible;
        }

        private void Quit()
        {
            SceneManager.LoadScene(BuildConfiguration.Instance.ClientMenuScene);
        }

    }
}