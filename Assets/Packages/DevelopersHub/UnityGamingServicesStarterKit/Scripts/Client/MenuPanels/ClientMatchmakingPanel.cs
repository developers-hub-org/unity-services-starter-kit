using System.Collections;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using System;
using TMPro;
using Unity.Services.Authentication;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientMatchmakingPanel : CanvasPanel
    {

        [SerializeField] private int _maxPlayers = 2;
        [SerializeField] private Button _buttonStart = null;
        [SerializeField] private Button _buttonCancel = null;
        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private TMP_Dropdown _dropdownMaps = null;
        [SerializeField] private GameObject _readyEffects = null;
        [SerializeField] private GameObject _matchmakingEffects = null;
        private CancellationTokenSource _cancellationToken;
        private bool _matchmaking = false;

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
            Hide();
        }

        private void Start()
        {
            _buttonStart.onClick.AddListener(StartMatchmaking);
            _buttonCancel.onClick.AddListener(CancelMatchmaking);
            _buttonBack.onClick.AddListener(Back);
        }

        private void OnDestroy()
        {
            _cancellationToken?.Dispose();
        }

        public override void Show()
        {
            base.Show();
            ResetUI();
        }

        public override void Hide()
        {
            base.Hide();
        }

        private async void StartMatchmaking()
        {
            if (_matchmaking) { return; }
            _matchmaking = true;
            ResetUI();
            _cancellationToken = new CancellationTokenSource();
            try
            {
                var map = ResourceManager.Instance.maps[_dropdownMaps.value];
                var options = new MatchmakerOptions();
                options.QueueName = map.matchmaker;
                options.TicketAttributes = new Dictionary<string, object>
                {
                    { "map", _dropdownMaps.value },
                    { "mode", 0 }
                };

                ClientMigrationHandler migrationHandler = ClientMigrationHandler.CreateInstance();
                var sessionOptions = new SessionOptions().WithHostMigration(migrationHandler);
                sessionOptions.MaxPlayers = _maxPlayers;
                sessionOptions.IsPrivate = true;

                sessionOptions.SessionProperties = new Dictionary<string, SessionProperty>
                {
                    { "map", new SessionProperty(_dropdownMaps.value.ToString(), VisibilityPropertyOptions.Public) },
                    { "started", new SessionProperty("1", VisibilityPropertyOptions.Public) },
                    { "mode", new SessionProperty("0", VisibilityPropertyOptions.Public) },
                    { "matchmaker", new SessionProperty("1", VisibilityPropertyOptions.Public) },
                    { "ended", new SessionProperty("0", VisibilityPropertyOptions.Public) }
                };
                
                sessionOptions.PlayerProperties = new Dictionary<string, PlayerProperty>
                {
                    { "name", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member) },
                    { "state", new PlayerProperty(((int)ClientSession.State.None).ToString(), VisibilityPropertyOptions.Member) },
                    { "team", new PlayerProperty("0", VisibilityPropertyOptions.Member) },
                };

                var session = await MultiplayerService.Instance.MatchmakeSessionAsync(options, sessionOptions, _cancellationToken.Token);
                ClientSession.Instance.RegisterMigrationHandler(session.Id, migrationHandler);
                CanvasPanel.Get<ClientSessionStartPanel>().Show(session, true);
            }
            catch (OperationCanceledException)
            {

            }
            catch (SessionException e)
            {
                if (e.Error != SessionError.MatchmakerCancelled)
                {
                    MessageBox.MessageBox.Show("Matchmaking Error", e.Message, "OK");
                    Debug.Log(e.Message);
                }
            }
            catch (Exception e)
            {
                MessageBox.MessageBox.Show("Matchmaking Error", "Failed to find a match to join.", "OK");
                Debug.Log(e.Message);
            }
            finally
            {
                _matchmaking = false;
                ResetUI();
            }
        }

        private void CancelMatchmaking()
        {
            if (!_matchmaking) { return; }
            _buttonCancel.interactable = false;
            _cancellationToken?.Cancel();
        }

        private void ResetUI()
        {
            _readyEffects.gameObject.SetActive(!_matchmaking);
            _matchmakingEffects.gameObject.SetActive(_matchmaking);
            _buttonStart.interactable = true;
            _buttonCancel.interactable = true;
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

    }
}