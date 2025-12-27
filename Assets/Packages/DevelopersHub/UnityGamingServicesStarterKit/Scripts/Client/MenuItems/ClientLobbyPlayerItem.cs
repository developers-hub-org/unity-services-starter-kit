using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLobbyPlayerItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textRow = null;
        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textTeam = null;
        [SerializeField] private Button _buttonProfile = null;
        [SerializeField] private Button _buttonKick = null;
        [SerializeField] private Color _hostColor = Color.yellow;
        [SerializeField] private Color _notReadyColor = Color.white;
        [SerializeField] private Color _readyColor = Color.green;

        private IReadOnlyPlayer _player = null; public string id => _player.Id;
        private ISession _session = null;

        private void Start()
        {
            _buttonProfile.onClick.AddListener(Profile);
            _buttonKick.onClick.AddListener(Kick);
        }

        public void Initialize(int row, ISession session, IReadOnlyPlayer player)
        {
            _textRow.text = row.ToString();
            Initialize(session, player);
        }

        public void Initialize(ISession session, IReadOnlyPlayer player)
        {
            _session = session;
            _player = player;
            _textName.text = _player.Properties.ContainsKey("name") ? _player.Properties["name"].Value : "Unknown";
            _textTeam.text = "~";
            bool ready = false;
            if (player.Properties != null && player.Properties.ContainsKey("ready"))
            {
                ready = player.Properties["ready"].Value == "1";
            }
            _buttonKick.gameObject.SetActive(player.Id != session.Host && session.Host == AuthenticationService.Instance.PlayerId);
            if (session.Host == _player.Id)
            {
                _textName.color = _hostColor;
            }
            else
            {
                _textName.color = ready ? _readyColor : _notReadyColor;
            }
        }

        public void UpdateRow(int row, ISession session)
        {
            _session = session;
            _textRow.text = row.ToString();
        }

        private void Profile()
        {
            CanvasPanel.Get<ClientProfilePanel>().Show(_player.Id, _player.Properties.ContainsKey("name") ? _player.Properties["name"].Value : "Unknown");
        }

        private async void Kick()
        {
            var block = ScreenBlock.Show();
            try
            {
                var host = _session.AsHost();
                await host.RemovePlayerAsync(_player.Id);
                block.Close();
                CanvasPanel.Get<ClientLobbyPanel>().UpdateRows();
                Destroy(gameObject);
            }
            catch (SessionException exception)
            {
                block.Close();
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
        }

    }
}