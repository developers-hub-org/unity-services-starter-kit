using System;
using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientSettingsPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private Tab[] _tabs = null;

        [Space]
        [Header("Account")]
        [SerializeField] private TMP_InputField _inputName = null;
        [SerializeField] private Button _buttonChangeName = null;
        [SerializeField] private Button _textButtonSaveName = null;
        [SerializeField] private Button _buttonSignout = null;
        [SerializeField] private TMP_InputField _inputUsername = null;
        [SerializeField] private Button _buttonSecure = null;
        [SerializeField] private Button _buttonChangePassword = null;

        [Serializable]
        private class Tab
        {
            public Button button = null;
            public RectTransform panel = null;
        }

        private void Awake()
        {
            Hide();
            if (_tabs != null)
            {
                for (int i = 0; i < _tabs.Length; i++)
                {
                    Tab tab = _tabs[i];
                    tab.button.onClick.AddListener(() => OnTabClick(tab));
                    tab.panel.gameObject.SetActive(false);
                }
            }
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
            _buttonSignout.onClick.AddListener(SignOut);
            _buttonChangeName.onClick.AddListener(ChangeName);
            _textButtonSaveName.onClick.AddListener(SaveName);
            _buttonSecure.onClick.AddListener(SecureAccount);
            _buttonChangePassword.onClick.AddListener(ChangePassword);
        }

        private void OnTabClick(Tab tab)
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].button.interactable = _tabs[i] != tab;
                _tabs[i].panel.gameObject.SetActive(_tabs[i] == tab);
            }
            SetupAccountTab();
        }

        public override void Show()
        {
            base.Show();
            if (_tabs != null && _tabs.Length > 0)
            {
                OnTabClick(_tabs[0]);
            }
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

        public void SetupAccountTab()
        {
            _inputName.text = AuthenticationService.Instance.PlayerName;
            _inputName.interactable = false;
            _inputUsername.interactable = false;
            _buttonChangeName.gameObject.SetActive(true);
            _textButtonSaveName.gameObject.SetActive(false);
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo.Username))
            {
                _inputUsername.text = "Account is not secure";
                _buttonChangePassword.gameObject.SetActive(false);
                _buttonSecure.gameObject.SetActive(true);
            }
            else
            {
                _inputUsername.text = AuthenticationService.Instance.PlayerInfo.Username;
                _buttonChangePassword.gameObject.SetActive(true);
                _buttonSecure.gameObject.SetActive(false);
            }
        }

        private void ChangeName()
        {
            SetupAccountTab();
            _buttonChangeName.gameObject.SetActive(false);
            _textButtonSaveName.gameObject.SetActive(true);
            _inputName.text = GetPlayerName();
            _inputName.interactable = true;
        }

        private async void SaveName()
        {
            string playerName = _inputName.text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }
            if (playerName != GetPlayerName())
            {
                var block = ScreenBlock.Show();
                try
                {
                    var response = await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                }
                catch
                {
                    MessageBox.MessageBox.Show("Error", "Failed to change the player name.", "OK");
                }
                block.Close();
            }
            SetupAccountTab();
        }

        private void SecureAccount()
        {
            CanvasPanel.Show<ClientSecureAccountPanel>(false);
        }

        private void ChangePassword()
        {
            CanvasPanel.Show<ClientChangePasswordPanel>(false);
        }

        private void SignOut()
        {
            string message = "Are you sure you want to sign out?";
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo.Username))
            {
                message += "You have not secured your account and you will lose it if you sign out.";
            }
            QuestionBoxBool.Show("Sign Out", message, "Yes", "No", SignoutConfirm);
        }

        private void SignoutConfirm(bool confirmed)
        {
            if (confirmed)
            {
                AuthenticationService.Instance.SignOut(true);
            }
        }

        private string GetPlayerName()
        {
            var username = AuthenticationService.Instance.PlayerName;
            if (!string.IsNullOrEmpty(username))
            {
                int index = username.IndexOf('#');
                if (index >= 0)
                {
                    username = username.Remove(index);
                }
            }
            return username;
        }

    }
}