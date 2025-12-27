using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientSecureAccountPanel : CanvasPanel
    {

        [SerializeField] private TMP_InputField _inputUsername = null;
        [SerializeField] private TMP_InputField _inputPassword = null;
        [SerializeField] private Button _buttonSecure = null;
        [SerializeField] private Button _buttonCancel = null;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonSecure.onClick.AddListener(Secure);
            _buttonCancel.onClick.AddListener(Cancel);
            _inputUsername.characterLimit = ClientLoginPanel.MaxUsernameCharacters;
            _inputPassword.characterLimit = ClientLoginPanel.MaxPasswordCharacters;
        }

        private async void Secure()
        {
            string username = _inputUsername.text.Trim();
            string password = _inputPassword.text.Trim();
            if (string.IsNullOrEmpty(username) || username.Length < ClientLoginPanel.MinUsernameCharacters || username.Length > ClientLoginPanel.MaxUsernameCharacters)
            {
                MessageBox.MessageBox.Show("Username Error", $"Username length must be min {ClientLoginPanel.MinUsernameCharacters} characters.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < ClientLoginPanel.MinPasswordCharacters || password.Length > ClientLoginPanel.MaxPasswordCharacters)
            {
                MessageBox.MessageBox.Show("Password Error", $"Password length must be min {ClientLoginPanel.MinPasswordCharacters} characters.", "OK");
                return;
            }
            var block = ScreenBlock.Show();
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
                Hide();
                MessageBox.MessageBox.Show("Success", "Account secured successfully.", "OK", SecureFinish);
            }
            catch
            {
                MessageBox.MessageBox.Show("Error", "Failed to secure account. Username can contain only letters, digits and symbols among {., -, _, @} and password must contain at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol.", "OK");
            }
            block.Close();
        }

        private void SecureFinish()
        {
            CanvasPanel.Get<ClientSettingsPanel>().SetupAccountTab();
        }

        private void Cancel()
        {
            Hide();
        }

        public override void Show()
        {
            base.Show();
            _inputUsername.text = "";
            _inputPassword.text = "";
        }

    }
}