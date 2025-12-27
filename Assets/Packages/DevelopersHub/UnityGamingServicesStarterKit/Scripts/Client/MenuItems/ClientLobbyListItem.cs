using System.Collections.Generic;
using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLobbyListItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textRow = null;
        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textHost = null;
        [SerializeField] private TextMeshProUGUI _textProtection = null;
        [SerializeField] private TextMeshProUGUI _textPlayers = null;
        [SerializeField] private Button _buttonJoin = null;
        [SerializeField] private Button _buttonEnter = null;

        private ISessionInfo _sessionInfo = null;
        private ISession _session = null;

        private void Start()
        {
            _buttonJoin.onClick.AddListener(Join);
            _buttonEnter.onClick.AddListener(Enter);
        }

        public void Initialize(int row, ISessionInfo session)
        {
            _sessionInfo = session;
            _textRow.text = row.ToString();
            _textName.text = session.Name;
            _textHost.text = session.Properties.ContainsKey("host") ? session.Properties["host"].Value : "Unknown";
            _textProtection.text = session.HasPassword ? "Private" : "Public";
            _textPlayers.text = (session.MaxPlayers - session.AvailableSlots).ToString() + "/" + session.MaxPlayers.ToString();
            bool member = false;
            _session = null;
            foreach (var item in MultiplayerService.Instance.Sessions.Values)
            {
                if(item.Id == session.Id)
                {
                    foreach (var player in item.Players)
                    {
                        if (player.Id == AuthenticationService.Instance.PlayerId)
                        {
                            member = true;
                            _session = item;
                            break;
                        }
                    }
                }
            }
            _buttonJoin.gameObject.SetActive(!member);
            _buttonEnter.gameObject.SetActive(member);
        }

        public void Initialize(int row, ISession session)
        {
            _session = session;
            _sessionInfo = null;
            _textRow.text = row.ToString();
            _textName.text = session.Name;
            _textHost.text = session.Properties.ContainsKey("host") ? session.Properties["host"].Value : "Unknown";
            _textProtection.text = session.HasPassword ? "Private" : "Public";
            _textPlayers.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();
            _buttonJoin.gameObject.SetActive(!session.IsMember);
            _buttonEnter.gameObject.SetActive(session.IsMember);
        }

        private void Join()
        {
            string id = null;
            bool password = false;
            if (_sessionInfo != null)
            {
                id = _sessionInfo.Id;
                password = _sessionInfo.HasPassword;
            }
            else if (_session != null)
            {
                id = _session.Id;
                password = _session.HasPassword;
            }
            if (password)
            {
                QuestionBoxString.Show("Password Required", "This lobby is private. Please enter the password to join.", "", "Password ...", 8, 20, "Join", "Cancel", TMP_InputField.ContentType.Password, (confirmed, enteredPassword) =>
                {
                    if (confirmed)
                    {
                        JoinConfirm(id, enteredPassword);
                    }
                });
            }
            else
            {
                JoinConfirm(id, null);
            }
        }

        private async void JoinConfirm(string id, string password)
        {
            var block = ScreenBlock.Show();
            try
            {
                ClientMigrationHandler migrationHandler = ClientMigrationHandler.CreateInstance();
                var options = new JoinSessionOptions().WithHostMigration(migrationHandler);
                options.PlayerProperties ??= new Dictionary<string, PlayerProperty>();
                options.Password = password;

                // Add player properties individually
                options.PlayerProperties.Add("id", new PlayerProperty(AuthenticationService.Instance.PlayerId, VisibilityPropertyOptions.Member));
                options.PlayerProperties.Add("name", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member));
                options.PlayerProperties.Add("state", new PlayerProperty(((int)ClientSession.State.None).ToString(), VisibilityPropertyOptions.Member));
                options.PlayerProperties.Add("team", new PlayerProperty("0", VisibilityPropertyOptions.Member));

                var session = await MultiplayerService.Instance.JoinSessionByIdAsync(id, options);
                ClientSession.Instance.RegisterMigrationHandler(session.Id, migrationHandler);
                block.Close();
                CanvasPanel.Hide<ClientLobbyListPanel>();
                CanvasPanel.Get<ClientLobbyPanel>().Show(session);
            }
            catch (SessionException exception)
            {
                block.Close();
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
        }

        private void Enter()
        {
            if (_session != null)
            {
                CanvasPanel.Hide<ClientLobbyListPanel>();
                CanvasPanel.Get<ClientLobbyPanel>().Show(_session);
            }
        }

    }
}