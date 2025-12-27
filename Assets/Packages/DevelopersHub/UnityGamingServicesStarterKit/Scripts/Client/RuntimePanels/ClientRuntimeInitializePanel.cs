using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeInitializePanel : CanvasPanel
    {

        [SerializeField] private uint _waitTime = 60;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientRuntimeInitializePlayerItem _itemsPrefab = null;
        private bool _started = false;
        private bool _initialized = false;
        private float _timer = 0;
        private bool _autoStart = false;
        private bool _eventsRegistered = false;
        public Dictionary<string, string> _players = new Dictionary<string, string>();

        private void Start()
        {
            ClientTerminal.OnLocalClientConnected += OnClientInitialized;
            _initialized = false;
            if (SessionManager.Instance.role == SessionManager.Role.Client)
            {
                if (SessionManager.Instance.type == SessionManager.ServerType.Session)
                {
                    Show();
                    Initialize();
                }
                else if (SessionManager.Instance.type == SessionManager.ServerType.Persistent)
                {
                    Hide();
                    StartCoroutine(StartPersistentAsync());
                }
            }
            else if (SessionManager.Instance.role == SessionManager.Role.Server)
            {
                Hide();
                StartCoroutine(StartPersistentAsync());
            }
        }

        private void OnClientInitialized()
        {
            ClientTerminal.OnLocalClientConnected -= OnClientInitialized;
            _started = true;
        }

        private IEnumerator StartPersistentAsync()
        {
            if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsListening)
            {
                // This was causing issues and seems unnecessary
                //NetworkManager.Singleton.Shutdown();
                //yield return new WaitForEndOfFrame();
                while (NetworkManager.Singleton.ShutdownInProgress)
                {
                    yield return null;
                }
            }
            if (SessionManager.Instance.role == SessionManager.Role.Client)
            {
                NetworkManager.Singleton.OnClientStarted += OnClientStarted;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                var transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
                transport.ConnectionData.Address = SessionManager.Instance.ip;
                transport.ConnectionData.Port = SessionManager.Instance.port;
                _started = NetworkManager.Singleton.StartClient();
                if (!_started)
                {
                    CanvasPanel.Show<ClientRuntimeDisconnectPanel>();
                }
            }
            else if (SessionManager.Instance.role == SessionManager.Role.Server)
            {
                var transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
                transport.ConnectionData.Address = SessionManager.Instance.ip;
                transport.ConnectionData.Port = SessionManager.Instance.port;
                _started = NetworkManager.Singleton.StartServer();
            }
        }

        private void OnClientDisconnectCallback(ulong id)
        {
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                CanvasPanel.Show<ClientRuntimeDisconnectPanel>();
            }
        }

        private void OnClientStarted()
        {

        }

        private void Update()
        {
            if (_started || !_autoStart || !_initialized) { return; }
            if (SessionManager.Instance.role == SessionManager.Role.Client)
            {
                _timer -= Time.unscaledDeltaTime;
                if (_timer <= 0)
                {
                    if (SessionManager.Instance.session.Host == AuthenticationService.Instance.PlayerId)
                    {
                        StartSession();
                    }
                }
            }
        }

        public override void Show()
        {
            base.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _started = false;
            if (SessionManager.Instance.role == SessionManager.Role.Client)
            {
                if(SessionManager.Instance.type == SessionManager.ServerType.Session)
                {
                    _timer = _waitTime;
                    _autoStart = _timer > 0;
                    InstantiatePlayers();
                    CheckStart();
                    if (!_eventsRegistered)
                    {
                        _eventsRegistered = true;
                        SessionManager.Instance.session.PlayerPropertiesChanged += PlayerPropertiesChanged;
                        SessionManager.Instance.session.PlayerLeaving += PlayerLeft;
                        SessionManager.Instance.session.SessionHostChanged += SessionHostChanged;
                        SessionManager.Instance.session.SessionPropertiesChanged += SessionPropertiesChanged;
                        SessionManager.Instance.session.PlayerJoined += PlayerJoined;
                        SessionManager.Instance.session.SessionMigrated += SessionMigrated;
                    }
                }
            }
        }

        private void SessionMigrated()
        {
            Debug.Log("SessionMigrated");
        }

        private void PlayerJoined(string id)
        {
            foreach (var player in ClientSession.Instance.StartedSession.Players)
            {
                if(_players.ContainsKey(player.Id)) { continue; }
                _players.Add(player.Id, player.Properties["name"].Value);
            }
        }

        private void SessionPropertiesChanged()
        {
            if (SessionManager.Instance.session.Properties["ended"].Value == "1")
            {
                var panel = CanvasPanel.Get<ClientRuntimeCompletePanel>();
                if (!panel.IsVisible)
                {
                    panel.Show();
                }
            }
        }

        private void SessionHostChanged(string id)
        {
            CheckStart();
        }

        private void InstantiatePlayers()
        {
            ClearItems();
            foreach (var player in SessionManager.Instance.session.Players)
            {
                var item = Instantiate(_itemsPrefab, _itemsContainer);
                item.Initialize(player.Id, player.Properties["name"].Value);
                int state = 0;
                int.TryParse(player.Properties["state"].Value, out state);
                item.SetStatus(player.Id == AuthenticationService.Instance.PlayerId ? ClientSession.State.WaitingInRuntimeScene : (ClientSession.State)state);
            }
        }

        private void PlayerLeft(string id)
        {
            var player = GetItem(id);
            if (player != null)
            {
                Destroy(player.gameObject);
            }
            CheckStart();
        }

        private void PlayerPropertiesChanged()
        {
            foreach (var player in SessionManager.Instance.session.Players)
            {
                var item = GetItem(player.Id);
                if (item != null)
                {
                    int state = 0;
                    int.TryParse(player.Properties["state"].Value, out state);
                    item.SetStatus(player.Id == AuthenticationService.Instance.PlayerId ? ClientSession.State.WaitingInRuntimeScene : (ClientSession.State)state);
                }
            }
            CheckStart();
        }

        private void CheckStart()
        {
            if (_started || !SessionManager.Instance.session.IsHost)
            {
                return;
            } 
            
            // Check if all players are ready
            bool ready = true;
            foreach (var player in SessionManager.Instance.session.Players)
            {
                if (SessionManager.Instance.session.Host == player.Id)
                {
                    continue;
                }
                int state = 0;
                int.TryParse(player.Properties["state"].Value, out state);
                if ((ClientSession.State)state != ClientSession.State.WaitingInRuntimeScene)
                {
                    ready = false;
                    break;
                }
            }

            if (ready)
            {
                StartSession();
            }
        }

        private async void StartSession(int tryCount = 0)
        {
            if (_started || !SessionManager.Instance.session.IsHost)
            {
                return;
            }
            _autoStart = false;
            _started = true;
            try
            {
                if(BuildConfiguration.Instance.ClientSessionType == SessionManager.SessionType.Relay)
                {
                    var options = new RelayNetworkOptions(RelayProtocol.DTLS);
                    await SessionManager.Instance.session.AsHost().Network.StartRelayNetworkAsync(options);
                }
                else if (BuildConfiguration.Instance.ClientSessionType == SessionManager.SessionType.DistributedAuthority)
                {
                    var options = new RelayNetworkOptions(RelayProtocol.Default);
                    await SessionManager.Instance.session.AsHost().Network.StartDistributedAuthorityNetworkAsync(options);
                }
            }
            catch (SessionException e)
            {
                _started = false; 
                Debug.Log(e.Error.ToString() + "\n" + e.Message);
                await Task.Delay(5000);
                tryCount += 1;
                if (tryCount <= 3)
                {
                    if (e.Error == SessionError.AllocationAlreadyExists)
                    {
                        try
                        {
                            
                        }
                        catch (SessionException e2)
                        {
                            Debug.Log(e2.Message);
                        }
                    }
                    else
                    {
                        StartSession(tryCount);
                    }
                }
            }
        }

        private async void Initialize(int tryCount = 0)
        {
            foreach (var player in ClientSession.Instance.StartedSession.Players)
            {
                _players.Add(player.Id, player.Properties["name"].Value);
            }
            try
            {
                var stateProperty = new PlayerProperty(((int)ClientSession.State.WaitingInRuntimeScene).ToString(), VisibilityPropertyOptions.Member);
                ClientSession.Instance.StartedSession.CurrentPlayer.SetProperty("state", stateProperty);
                await ClientSession.Instance.StartedSession.SaveCurrentPlayerDataAsync();
                _initialized = true;
                CheckStart();
            }
            catch(SessionException e)
            {
                Debug.Log(e.Message);
                await Task.Delay(1000);
                tryCount += 1;
                if(tryCount <= 3)
                {
                    Initialize(tryCount);
                }            
            }
        }

        private ClientRuntimeInitializePlayerItem GetItem(string id)
        {
            var items = _itemsContainer.GetComponentsInChildren<ClientRuntimeInitializePlayerItem>(true);
            if (items != null)
            {
                foreach (var item in items)
                {
                    if(item.id == id)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private void ClearItems()
        {
            var items = _itemsContainer.GetComponentsInChildren<ClientRuntimeInitializePlayerItem>(true);
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
            try
            {
                Hide();
                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                ClientTerminal.OnLocalClientConnected -= OnClientInitialized;
            }
            catch { }
            try
            {
                if (SessionManager.Instance.role == SessionManager.Role.Client)
                {
                    SessionManager.Instance.session.PlayerPropertiesChanged -= PlayerPropertiesChanged;
                    SessionManager.Instance.session.PlayerLeaving -= PlayerLeft;
                    SessionManager.Instance.session.SessionHostChanged -= SessionHostChanged;
                    SessionManager.Instance.session.SessionPropertiesChanged -= SessionPropertiesChanged;
                    SessionManager.Instance.session.SessionMigrated -= SessionMigrated;
                }
                else
                {

                }
            }
            catch { }
        }

    }
}