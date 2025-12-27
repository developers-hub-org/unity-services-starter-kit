using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLoginPanel : CanvasPanel
    {

        [SerializeField] private TMP_InputField _inputUsername = null;
        [SerializeField] private TMP_InputField _inputPassword = null;
        [SerializeField] private Button _buttonLogin = null;
        [SerializeField] private Button _buttonBack = null;

        private static readonly int _minUsernameCharacters = 3; public static int MinUsernameCharacters { get { return _minUsernameCharacters; } }
        private static readonly int _maxUsernameCharacters = 20; public static int MaxUsernameCharacters { get { return _maxUsernameCharacters; } }
        private static readonly int _minPasswordCharacters = 8; public static int MinPasswordCharacters { get { return _minPasswordCharacters; } }
        private static readonly int _maxPasswordCharacters = 30; public static int MaxPasswordCharacters { get { return _maxPasswordCharacters; } }

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonLogin.onClick.AddListener(Login);
            _buttonBack.onClick.AddListener(Back);
            _inputUsername.characterLimit = MaxUsernameCharacters;
            _inputPassword.characterLimit = MaxPasswordCharacters;
        }

        private void Login()
        {
            string username = _inputUsername.text.Trim();
            string password = _inputPassword.text.Trim();
            if (string.IsNullOrEmpty(username) || username.Length < MinUsernameCharacters || username.Length > MaxUsernameCharacters)
            {
                MessageBox.MessageBox.Show("Username Error", "Username is not valid.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordCharacters || password.Length > MaxPasswordCharacters)
            {
                MessageBox.MessageBox.Show("Password Error", "Password is not valid.", "OK");
                return;
            }
            ClientAuthentication.Instance.SignInWithUsernamePasswordAsync(username, password);
        }

        private void Back()
        {
            CanvasPanel.Show<ClientStartupPanel>();
        }

    }
}