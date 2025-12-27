using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLobbyPanel : CanvasPanel
    {

        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textMap = null;
        [SerializeField] private TextMeshProUGUI _textJoinCode = null;
        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private Button _buttonLeave = null;
        [SerializeField] private Button _buttonReady = null;
        [SerializeField] private Button _buttonNotReady = null;
        [SerializeField] private Button _buttonStart = null;
        [SerializeField] private Button _buttonEdit = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientLobbyPlayerItem _itemsPrefab = null;

        private ISession _session = null;
        private int _row = 0;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
            _buttonLeave.onClick.AddListener(Leave);
            _buttonEdit.onClick.AddListener(Edit);
            _buttonReady.onClick.AddListener(Ready);
            _buttonNotReady.onClick.AddListener(NotReady);
            _buttonStart.onClick.AddListener(StartSession);
        }

        private async void StartSession()
        {
            var block = ScreenBlock.Show();
            try
            {
                var hostSession = _session.AsHost();

                var playerProperty = new PlayerProperty(((int)ClientSession.State.StartedSession).ToString(), VisibilityPropertyOptions.Member);
                hostSession.CurrentPlayer.SetProperty("started", playerProperty);
                await hostSession.SaveCurrentPlayerDataAsync();

                var sessionProperty = new SessionProperty("1", VisibilityPropertyOptions.Public);
                hostSession.SetProperty("started", sessionProperty);
                hostSession.IsPrivate = true;
                await hostSession.SavePropertiesAsync();

                block.Close();
                int mapIndex = 0;
                if (_session.Properties.ContainsKey("map"))
                {
                    int.TryParse(_session.Properties["map"].Value, out mapIndex);
                }

                SessionManager.Instance.InitializeSession(_session);
                ClientSession.Instance.InitializeSession(_session);
                CanvasPanel.HideAll();
                CanvasPanel.Get<ClientLoadingPanel>().Show(ResourceManager.Instance.maps[mapIndex].scene);
            }
            catch (SessionException e)
            {
                MessageBox.MessageBox.Show("Lobby Error", e.Message, "OK");
                Debug.Log(e.Message);
                block.Close();
            }
        }

        public void Show(ISession session)
        {
            ClearItems();
            Show();
            _session = session;
            _textName.text = session.Name;
            int mapIndex = 0;
            if (session.Properties.ContainsKey("map"))
            {
                int.TryParse(session.Properties["map"].Value, out mapIndex);
            }
            bool started = session.Properties["started"].Value == "1";
            _textMap.text = ResourceManager.Instance.maps[mapIndex].name;
            _textJoinCode.text = session.IsHost ? "Join Code: " + session.Code : "";
            _buttonEdit.gameObject.SetActive(session.IsHost);
            _buttonStart.gameObject.SetActive(session.IsHost && !started);
            UpdateReady();
            UpdatePlayers();
            RegisterSessionEvents();
        }

        public override void Hide()
        {
            UnregisterSessionEvents();
            base.Hide();
        }

        private void UpdateReady()
        {
            bool ready = false;
            foreach (var player in _session.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    if (player.Properties != null && player.Properties.ContainsKey("ready"))
                    {
                        ready = player.Properties["ready"].Value == "1";
                    }
                    break;
                }
            }
            _buttonReady.gameObject.SetActive(!_session.IsHost && !ready);
            _buttonNotReady.gameObject.SetActive(!_session.IsHost && ready);
        }

        private void UpdatePlayers()
        {
            ClearItems();
            foreach (var player in _session.Players)
            {
                _row += 1;
                var item = Instantiate(_itemsPrefab, _itemsContainer);
                item.Initialize(_row, _session, player);
            }
        }

        private void Back()
        {
            CanvasPanel.Show<ClientLobbyListPanel>();
        }

        private async void Leave()
        {
            var block = ScreenBlock.Show();
            try
            {
                string id = _session.Id;
                await _session.LeaveAsync();
                block.Close();
                CanvasPanel.Get<ClientLobbyListPanel>().LeftSession(id);
                CanvasPanel.Show<ClientLobbyListPanel>();
            }
            catch (SessionException exception)
            {
                block.Close();
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
        }

        private void Edit()
        {
            CanvasPanel.Get<ClientLobbySettingsPanel>().Show(_session);
        }

        private async void Ready()
        {
            var block = ScreenBlock.Show();
            try
            {
                var readyProperty = new PlayerProperty("1", VisibilityPropertyOptions.Member);
                _session.CurrentPlayer.SetProperty("ready", readyProperty);
                await _session.SaveCurrentPlayerDataAsync();
                _buttonReady.gameObject.SetActive(false);
                _buttonNotReady.gameObject.SetActive(true);
            }
            catch (SessionException exception)
            {
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
            block.Close();
        }

        private async void NotReady()
        {
            var block = ScreenBlock.Show();
            try
            {
                var readyProperty = new PlayerProperty("0", VisibilityPropertyOptions.Member);
                _session.CurrentPlayer.SetProperty("ready", readyProperty);
                await _session.SaveCurrentPlayerDataAsync();
                _buttonReady.gameObject.SetActive(true);
                _buttonNotReady.gameObject.SetActive(false);
            }
            catch (SessionException exception)
            {
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
            block.Close();
        }

        public void UpdateRows()
        {
            _row = 0;
            var players = _itemsContainer.GetComponentsInChildren<ClientLobbyPlayerItem>();
            if (players != null)
            {
                foreach (var item in players)
                {
                    _row += 1;
                    item.UpdateRow(_row, _session);
                }
            }
        }

        private void ClearItems()
        {
            _row = 0;
            var items = _itemsContainer.GetComponentsInChildren<ClientLobbyPlayerItem>(true);
            if (items != null)
            {
                foreach (var item in items)
                {
                    Destroy(item.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            UnregisterSessionEvents();
        }

        private void RegisterSessionEvents()
        {
            _session.PlayerJoined += PlayerJoined;
            _session.PlayerHasLeft += PlayerHasLeft;
            _session.PlayerPropertiesChanged += PlayerPropertiesChanged;
            _session.StateChanged += StateChanged;
            _session.Changed += SessionChanged;
            _session.SessionPropertiesChanged += SessionPropertiesChanged;
            _session.SessionHostChanged += SessionHostChanged;
            _session.Deleted += SessionDeleted;
            _session.RemovedFromSession += RemovedFromSession;
            _session.SessionMigrated += SessionMigrated;
            _session.PlayerLeaving += PlayerLeaving;
        }

        private void UnregisterSessionEvents()
        {
            try
            {
                _session.PlayerJoined -= PlayerJoined;
                _session.PlayerHasLeft -= PlayerHasLeft;
                _session.PlayerPropertiesChanged -= PlayerPropertiesChanged;
                _session.StateChanged -= StateChanged;
                _session.Changed -= SessionChanged;
                _session.SessionPropertiesChanged -= SessionPropertiesChanged;
                _session.SessionHostChanged -= SessionHostChanged;
                _session.Deleted -= SessionDeleted;
                _session.RemovedFromSession -= RemovedFromSession;
                _session.SessionMigrated -= SessionMigrated;
                _session.PlayerLeaving -= PlayerLeaving;
            }
            catch { }
        }

        private void PlayerHasLeft(string id)
        {
            _row = 0;
            var players = _itemsContainer.GetComponentsInChildren<ClientLobbyPlayerItem>();
            if (players != null)
            {
                foreach (var item in players)
                {
                    if (item.id == id)
                    {
                        Destroy(item.gameObject);
                    }
                    else
                    {
                        _row += 1;
                        item.UpdateRow(_row, _session);
                    }
                }
            }
        }

        private void PlayerJoined(string id)
        {
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if (session.Id == _session.Id)
                {
                    _session = session;
                    break;
                }
            }
            var player = _session.GetPlayer(id);
            _row += 1;
            var item = Instantiate(_itemsPrefab, _itemsContainer);
            item.Initialize(_row, _session, player);
        }

        private void PlayerPropertiesChanged()
        {
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if (session.Id == _session.Id)
                {
                    _session = session;
                    break;
                }
            }
            var players = _itemsContainer.GetComponentsInChildren<ClientLobbyPlayerItem>();
            if (players != null)
            {
                foreach (var item in players)
                {
                    var player = _session.GetPlayer(item.id);
                    if (player != null)
                    {
                        item.Initialize(_session, player);
                    }
                }
            }
        }

        private void PlayerLeaving(string id)
        {

        }

        private void RemovedFromSession()
        {
            Back();
        }

        private void SessionMigrated()
        {

        }

        private void SessionDeleted()
        {
            Back();
        }

        private void SessionHostChanged(string id)
        {
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if (session.Id == _session.Id)
                {
                    _session = session;
                    break;
                }
            }
            if (id == AuthenticationService.Instance.PlayerId)
            {
                Show(_session);
            }
            else
            {
                var players = _itemsContainer.GetComponentsInChildren<ClientLobbyPlayerItem>();
                if (players != null)
                {
                    foreach (var item in players)
                    {
                        var player = _session.GetPlayer(item.id);
                        if (player != null)
                        {
                            item.Initialize(_session, player);
                        }
                    }
                }
            }
        }

        private void SessionPropertiesChanged()
        {

        }

        private void SessionChanged()
        {

        }

        private void StateChanged(SessionState state)
        {

        }

    }
}