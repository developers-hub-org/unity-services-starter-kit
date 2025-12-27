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
    public class ClientLobbyListPanel : CanvasPanel
    {

        [SerializeField] private int _loadCount = 10;
        [SerializeField] private float _minRefreshTime = 30f;
        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private Button _buttonCreate = null;
        [SerializeField] private Button _buttonLoadMore = null;
        [SerializeField] private Button _buttonRefresh = null;
        [SerializeField] private Button _buttonJoinCode = null;
        [SerializeField] private Toggle _toggleFilterJoined = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientLobbyListItem _itemsPrefab = null;

        private float _lastRefreshTime = 0f;
        private List<ISessionInfo> _sessions = new List<ISessionInfo>();
        private string _continuationToken = null;
        private int _row = 0;
        private bool _loading = false;

        private void Awake()
        {
            Hide();
            _lastRefreshTime = Time.realtimeSinceStartup - _minRefreshTime;
            _continuationToken = null;
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
            _buttonCreate.onClick.AddListener(Create);
            _buttonLoadMore.onClick.AddListener(LoadMore);
            _buttonRefresh.onClick.AddListener(Refresh);
            _buttonJoinCode.onClick.AddListener(JoinWithCode);
            _toggleFilterJoined.onValueChanged.AddListener(FilterOnlyJoinedLobies);
        }

        private void Update()
        {
            if (!IsVisible) { return; }
            if (!_buttonRefresh.interactable && !_loading && Time.realtimeSinceStartup - _lastRefreshTime >= _minRefreshTime)
            {
                _buttonRefresh.interactable = true;
            }
        }

        public override void Show()
        {
            base.Show();
            ClearItems();
            _toggleFilterJoined.SetIsOnWithoutNotify(false);
            ClientSession.Instance.Initialize(InitializeLobbyList);
        }

        private void InitializeLobbyList(bool initialized)
        {
            if (initialized)
            {
                InstantiateLobbyList();
            }
        }

        private void FilterOnlyJoinedLobies(bool filter)
        {
            InstantiateLobbyList();
        }

        private void InstantiateLobbyList()
        {
            if (_toggleFilterJoined.isOn)
            {
                ClearItems();
                foreach (var session in MultiplayerService.Instance.Sessions.Values)
                {
                    _row += 1;
                    var item = Instantiate(_itemsPrefab, _itemsContainer);
                    item.Initialize(_row, session);
                }
            }
            else
            {
                if (_sessions.Count > 0)
                {
                    ClearItems();
                    foreach (var session in _sessions)
                    {
                        _row += 1;
                        var item = Instantiate(_itemsPrefab, _itemsContainer);
                        item.Initialize(_row, session);
                    }
                }
                else if (!string.IsNullOrEmpty(_continuationToken))
                {
                    LoadMore();
                }
                else
                {
                    Refresh();
                }
            }
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

        private void Create()
        {
            CanvasPanel.Show<ClientLobbySettingsPanel>(false);
        }

        private void LoadMore()
        {
            _loading = true;
            ClientSession.Instance.Query(_loadCount, _continuationToken, OnSessionsLoaded);
        }

        private void Refresh()
        {
            ClearItems();
            _loading = true;
            _lastRefreshTime = Time.realtimeSinceStartup;
            _buttonRefresh.interactable = false;
            _buttonLoadMore.interactable = false;
            ClientSession.Instance.Query(_loadCount, null, OnSessionsLoaded);
        }

        private void OnSessionsLoaded(QuerySessionsResults sessions)
        {
            _loading = false;
            if (sessions != null)
            {
                foreach (var session in sessions.Sessions)
                {
                    _sessions.Add(session);
                    _row += 1;
                    var item = Instantiate(_itemsPrefab, _itemsContainer);
                    item.Initialize(_row, session);
                }
            }
            _continuationToken = (sessions != null && sessions.Sessions.Count >= _loadCount) ? sessions.ContinuationToken : null;
            _buttonLoadMore.interactable = !string.IsNullOrEmpty(_continuationToken);
        }

        private void JoinWithCode()
        {
            QuestionBoxString.Show("Join with Code", "Please enter the lobby code to join.", "", "Join Code ...", 3, 20, "Join", "Cancel", TMP_InputField.ContentType.Standard, JoinWithCodeConfirm);
        }

        private async void JoinWithCodeConfirm(bool confirmed, string code)
        {
            var block = ScreenBlock.Show();
            try
            {
                ClientMigrationHandler migrationHandler = ClientMigrationHandler.CreateInstance();
                var options = new JoinSessionOptions().WithHostMigration(migrationHandler);
                options.PlayerProperties ??= new Dictionary<string, PlayerProperty>();

                // Add player properties individually
                options.PlayerProperties.Add("name", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member));
                options.PlayerProperties.Add("state", new PlayerProperty(((int)ClientSession.State.None).ToString(), VisibilityPropertyOptions.Member));
                options.PlayerProperties.Add("team", new PlayerProperty("0", VisibilityPropertyOptions.Member));

                var session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options);
                ClientSession.Instance.RegisterMigrationHandler(session.Id, migrationHandler);
                block.Close();
                Hide();
                CanvasPanel.Get<ClientLobbyPanel>().Show(session);
            }
            catch (SessionException exception)
            {
                block.Close();
                MessageBox.MessageBox.Show("Lobby Error", exception.Message, "OK");
            }
        }

        public void LeftSession(string id)
        {
            for (int i = 0; i < _sessions.Count; i++)
            {
                if (_sessions[i].Id == id)
                {
                    _sessions.RemoveAt(i);
                    break;
                }
            }
        }

        private void ClearItems()
        {
            _row = 0;
            var items = _itemsContainer.GetComponentsInChildren<ClientLobbyListItem>(true);
            if (items != null)
            {
                foreach (var item in items)
                {
                    Destroy(item.gameObject);
                }
            }
        }

    }
}