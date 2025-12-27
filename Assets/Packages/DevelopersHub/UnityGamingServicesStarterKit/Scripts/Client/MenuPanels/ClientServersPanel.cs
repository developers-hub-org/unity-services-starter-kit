using System;
using DevelopersHub.MessageBox;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientServersPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientServersItem _itemsPrefab = null;
        private ServerList _servers = null;

        [System.Serializable]
        public class ServerList
        {
            public ServerInfo[] servers;
        }

        [System.Serializable]
        public class ServerInfo
        {
            public string name;
            public string ip;
            public int port;
            public string region;
            public int players;
            public int max_players;
            public string password;
            public int map;
        }

        public struct userAttributes { }
        public struct appAttributes { }

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

        public override void Show()
        {
            base.Show();
            ClearItems();
            if (_servers == null)
            {
                GetServersList();
            }
            else
            {
                InstantiateServerItems();
            }
        }

        private async void GetServersList()
        {
            var block = ScreenBlock.Show();
            try
            {
                await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
                string serverListJson = RemoteConfigService.Instance.appConfig.GetJson("servers");
                _servers = JsonUtility.FromJson<ServerList>(serverListJson);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to fetch server list: {e}");
                _servers = null;
            }
            block.Close();
            InstantiateServerItems();
        }

        private void InstantiateServerItems()
        {
            ClearItems();
            if (_servers != null)
            {
                for (int i = 0; i < _servers.servers.Length; i++)
                {
                    var server = _servers.servers[i];
                    ClientServersItem item = Instantiate(_itemsPrefab, _itemsContainer);
                    var map = ResourceManager.Instance.maps[server.map];
                    item.Initialize(i + 1, server.name, server.ip, server.port, server.region, server.players, server.max_players, server.password, map.name, map.scene);
                }
            }
        }

        private void ClearItems()
        {
            var items = _itemsContainer.GetComponentsInChildren<ClientServersItem>(true);
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