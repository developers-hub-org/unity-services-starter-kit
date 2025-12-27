using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientChangePasswordPanel : CanvasPanel
    {

        [SerializeField] private TMP_InputField _inputCurrentPassword = null;
        [SerializeField] private TMP_InputField _inputNewPassword = null;
        [SerializeField] private Button _buttonChange = null;
        [SerializeField] private Button _buttonCancel = null;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonChange.onClick.AddListener(Change);
            _buttonCancel.onClick.AddListener(Cancel);
        }

        private async void Change()
        {
            string currentPassword = _inputCurrentPassword.text.Trim();
            string password = _inputNewPassword.text.Trim();
            if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length < ClientLoginPanel.MinPasswordCharacters || currentPassword.Length > ClientLoginPanel.MaxPasswordCharacters)
            {
                MessageBox.MessageBox.Show("Current Password Error", $"Current password length must be min {ClientLoginPanel.MinPasswordCharacters} characters.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < ClientLoginPanel.MinPasswordCharacters || password.Length > ClientLoginPanel.MaxPasswordCharacters)
            {
                MessageBox.MessageBox.Show("New Password Error", "Password does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30 characters.", "OK");
                return;
            }
            var block = ScreenBlock.Show();
            try
            {
                await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, password);
                Hide();
                MessageBox.MessageBox.Show("Success", "Changed password successfully.", "OK", ChangeFinish);
            }
            catch
            {
                MessageBox.MessageBox.Show("Password Error", "Password does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30 characters.", "OK");
            }
            block.Close();
        }

        private void ChangeFinish()
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
            _inputCurrentPassword.text = "";
            _inputNewPassword.text = "";
        }

    }
}