using System.Collections;
using System.Collections.Generic;
using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLobbySettingsPanel : CanvasPanel
    {

        [SerializeField] private TMP_InputField _inputName = null;
        [SerializeField] private Toggle _togglePassword = null;
        [SerializeField] private TMP_InputField _inputPassword = null;
        [SerializeField] private TMP_InputField _inputPlayers = null;
        [SerializeField] private TMP_Dropdown _dropdownMaps = null;
        [SerializeField] private Button _buttonClose = null;
        [SerializeField] private Button _buttonSave = null;
        [SerializeField] private Button _buttonCreate = null;

        private ISession _session = null;

        private void Awake()
        {
            _dropdownMaps.ClearOptions();
            List<string> pageOptions = new List<string>();
            if (ResourceManager.Instance.maps != null && ResourceManager.Instance.maps.Length > 0)
            {
                for (int i = 0; i < ResourceManager.Instance.maps.Length; i++)
                {
                    pageOptions.Add(ResourceManager.Instance.maps[i].name);
                }
                _dropdownMaps.interactable = true;
            }
            else
            {
                pageOptions.Add("Select a Map");
                _dropdownMaps.interactable = false;
            }
            _dropdownMaps.AddOptions(pageOptions);
            _inputName.characterLimit = 20;
            _inputPassword.characterLimit = 20;
            Hide();
        }

        private void Start()
        {
            _togglePassword.onValueChanged.AddListener(ChangeProtection);
            _buttonClose.onClick.AddListener(Close);
            _buttonCreate.onClick.AddListener(Create);
            _buttonSave.onClick.AddListener(Edit);
        }

        private void ChangeProtection(bool protect)
        {
            _inputPassword.interactable = protect;
        }

        public override void Show()
        {
            base.Show();
            _togglePassword.interactable = true;
            _inputName.text = "";
            _inputPassword.text = "";
            _inputPlayers.text = "";
            _inputPassword.interactable = false;
            _togglePassword.SetIsOnWithoutNotify(false);
            _inputName.interactable = true;
            _inputPlayers.interactable = true;
            if (_dropdownMaps.options.Count > 0)
            {
                _dropdownMaps.SetValueWithoutNotify(0);
            }
            _buttonSave.gameObject.SetActive(false);
            _buttonCreate.gameObject.SetActive(true);
        }

        public void Show(ISession session)
        {
            Show();
            _session = session;
            _togglePassword.interactable = false;
            _inputName.text = session.Name;
            _inputName.interactable = false;
            _inputPassword.text = "";
            _inputPlayers.text = session.MaxPlayers.ToString();
            _inputPlayers.interactable = false;
            _inputPassword.interactable = false;
            _togglePassword.SetIsOnWithoutNotify(false);
            if (_dropdownMaps.options.Count > 0)
            {
                _dropdownMaps.SetValueWithoutNotify(0);
            }
            _buttonSave.gameObject.SetActive(true);
            _buttonCreate.gameObject.SetActive(false);
        }

        private void Close()
        {
            Hide();
        }
        
        private async void Create()
        {
            string name = _inputName.text.Trim();
            bool protect = _togglePassword.isOn;
            string password = protect ? _inputPassword.text.Trim() : null;
            int players = 0;
            int.TryParse(_inputPlayers.text.Trim(), out players);
            int map = _dropdownMaps.value;

            if (string.IsNullOrEmpty(name) || name.Length < 3)
            {
                MessageBox.MessageBox.Show("Invalid Name", "Please enter a valid lobby name with at least 3 characters.", "OK");
                return;
            }

            if (players < 2)
            {
                MessageBox.MessageBox.Show("Invalid Max Players", "Enter minimum value of 2 for the max players.", "OK");
                return;
            }

            ClientMigrationHandler migrationHandler = ClientMigrationHandler.CreateInstance();
            var options = new SessionOptions().WithHostMigration(migrationHandler);
            options.MaxPlayers = players;
            options.Name = name;
            options.Password = password;
            options.IsPrivate = false;
            options.IsLocked = false;

            // Initialize dictionaries if they don't exist
            options.SessionProperties ??= new Dictionary<string, SessionProperty>();
            options.PlayerProperties ??= new Dictionary<string, PlayerProperty>();

            // Add session properties individually
            options.SessionProperties.Add("map", new SessionProperty(map.ToString(), VisibilityPropertyOptions.Public));
            options.SessionProperties.Add("host", new SessionProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Public));
            options.SessionProperties.Add("started", new SessionProperty("0", VisibilityPropertyOptions.Public));
            options.SessionProperties.Add("mode", new SessionProperty("0", VisibilityPropertyOptions.Public));
            options.SessionProperties.Add("matchmaker", new SessionProperty("0", VisibilityPropertyOptions.Public));
            options.SessionProperties.Add("ended", new SessionProperty("0", VisibilityPropertyOptions.Public));

            // Add player properties individually
            options.PlayerProperties.Add("name", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member));
            options.PlayerProperties.Add("state", new PlayerProperty(((int)ClientSession.State.None).ToString(), VisibilityPropertyOptions.Member));
            options.PlayerProperties.Add("team", new PlayerProperty("0", VisibilityPropertyOptions.Member));

            var block = ScreenBlock.Show();
            try
            {
                var session = await MultiplayerService.Instance.CreateSessionAsync(options);
                ClientSession.Instance.RegisterMigrationHandler(session.Id, migrationHandler);
                Hide();
                CanvasPanel.Hide<ClientLobbyListPanel>();
                CanvasPanel.Get<ClientLobbyPanel>().Show(session);
            }
            catch (SessionException exception)
            {
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
            block.Close();
        }

        private async void Edit()
        {
            var hostSession = _session.AsHost();
            int map = _dropdownMaps.value;
            int sessionMap = 0;

            if (_session.Properties.ContainsKey("map"))
            {
                int.TryParse(_session.Properties["map"].Value, out sessionMap);
            }

            if (hostSession == null || map == sessionMap)
            {
                Close();
            }

            var block = ScreenBlock.Show();
            try
            {
                var mapProperty = new SessionProperty(map.ToString(), VisibilityPropertyOptions.Public);
                hostSession.SetProperty("map", mapProperty);
                await hostSession.SavePropertiesAsync();
                Hide();
            }
            catch (SessionException exception)
            {
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
            block.Close();
        }

    }
}