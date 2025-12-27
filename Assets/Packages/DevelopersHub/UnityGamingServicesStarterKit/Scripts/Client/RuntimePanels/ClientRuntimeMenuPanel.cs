using DevelopersHub.MessageBox;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeMenuPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonQuit = null;
        [SerializeField] private Button _buttonResume = null;

        private CursorLockMode _cursorLockMode = CursorLockMode.None;
        private bool _cursorVisible = true;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if(InputManager.Instance.Cancel)
            {
                if (CanvasPanel.Get<ClientRuntimeCompletePanel>()?.IsVisible == false && CanvasPanel.Get<ClientRuntimeDisconnectPanel>()?.IsVisible == false && CanvasPanel.Get<ClientRuntimeInitializePanel>()?.IsVisible == false)
                {
                    if (IsVisible)
                    {
                        Hide();
                        Cursor.lockState = _cursorLockMode;
                        Cursor.visible = _cursorVisible;
                    }
                    else
                    {
                        _cursorLockMode = Cursor.lockState;
                        _cursorVisible = Cursor.visible;
                        Show();
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }
            }
        }

        private void Start()
        {
            _buttonQuit.onClick.AddListener(Quit);
            _buttonResume.onClick.AddListener(Resume);
        }

        private void Quit()
        {
            QuestionBoxBool.Show("Quit Game", "Are you sure you want to quit the game?", "Yes", "No", QuitConfirm);
        }

        private void QuitConfirm(bool quit)
        {
            var block = ScreenBlock.Show();
            SessionManager.Instance.RuntimeLeaveSession();
            SceneManager.LoadScene(BuildConfiguration.Instance.ClientMenuScene);
        }

        private void Resume()
        {
            Hide();
        }

    }
}