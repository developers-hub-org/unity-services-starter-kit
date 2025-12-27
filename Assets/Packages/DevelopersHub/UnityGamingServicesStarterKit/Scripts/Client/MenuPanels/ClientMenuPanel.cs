using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientMenuPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonSettings = null;
        [SerializeField] private Button _buttonServers = null;
        [SerializeField] private Button _buttonFriends = null;
        [SerializeField] private Button _buttonLeaderboards = null;
        [SerializeField] private Button _buttonLobbies = null;
        [SerializeField] private Button _buttonMatchmaker = null;
        [SerializeField] private TextMeshProUGUI _textGold = null;
        [SerializeField] private TextMeshProUGUI _textDimond = null;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonSettings.onClick.AddListener(Settings);
            _buttonServers.onClick.AddListener(Servers);
            _buttonFriends.onClick.AddListener(Friends);
            _buttonLeaderboards.onClick.AddListener(Leaderboards);
            _buttonLobbies.onClick.AddListener(Lobbies);
            _buttonMatchmaker.onClick.AddListener(Matchmaker);
        }

        public override void Show()
        {
            base.Show();
            _buttonFriends.interactable = ClientFriends.Instance.IsInitialized;
            _buttonLeaderboards.interactable = !string.IsNullOrEmpty(BuildConfiguration.Instance.LeaderboardsId);
            _buttonServers.interactable = BuildConfiguration.Instance.EnablePersistentServers;
            _buttonLobbies.interactable = BuildConfiguration.Instance.EnableSessions;
            _textGold.text = "~";
            _textDimond.text = "~";
            if (BuildConfiguration.Instance.EnableFriendsService)
            {
                ClientFriends.Instance.Initialize(FriendsInitializeResponse);
            }
            if (BuildConfiguration.Instance.EnableSessions)
            {
                ClientSession.Instance.Initialize(SessionsInitializeResponse);
            }
            if (BuildConfiguration.Instance.EnableEconomy)
            {
                ClientEconomy.Instance.Initialize(EconomyInitializeResponse);
            }
        }

        private void EconomyInitializeResponse(bool initialized)
        {
            _textGold.text = ClientEconomy.Instance.GetCurrencyBalance("GOLD").ToString();
            _textDimond.text = ClientEconomy.Instance.GetCurrencyBalance("DIMOND").ToString();
        }

        private void FriendsInitializeResponse(bool initialized)
        {
            _buttonFriends.interactable = initialized;
        }

        private void SessionsInitializeResponse(bool initialized)
        {
            _buttonLobbies.interactable = initialized;
        }

        private void Friends()
        {
            CanvasPanel.Show<ClientFriendsPanel>();
        }

        private void Settings()
        {
            CanvasPanel.Show<ClientSettingsPanel>();
        }

        private void Servers()
        {
            CanvasPanel.Show<ClientServersPanel>();
        }

        private void Leaderboards()
        {
            CanvasPanel.Show<ClientLeaderboardsPanel>();
        }

        private void Lobbies()
        {
            CanvasPanel.Show<ClientLobbyListPanel>();
        }

        private void Matchmaker()
        {
            CanvasPanel.Show<ClientMatchmakingPanel>();
        }

    }
}