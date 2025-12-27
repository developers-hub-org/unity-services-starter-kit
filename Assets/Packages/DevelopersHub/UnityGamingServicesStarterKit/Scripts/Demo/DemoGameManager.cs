using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoGameManager : NetworkBehaviour
    {

        [SerializeField] private DemoCameraController cameraController = null;
        [SerializeField] private NetworkObject playerPrefab = null;

        private static DemoGameManager _instance = null; public static DemoGameManager Instance { get { return _instance; } }
        private Dictionary<string, long> _playersCoins = new Dictionary<string, long>();
        private List<ulong> _collectedCoinsId = new List<ulong>();

        private long _totalCoins = 0;
        private long _collectedCoins = 0;
        private bool _completed = false;

        private void Awake()
        {
            _instance = this;
            _totalCoins = FindObjectsByType<DemoCoin>(FindObjectsSortMode.None).Length;
            _collectedCoins = 0;
            ClientTerminal.OnLocalClientConnected += OnLocalClientConnected;
            ClientTerminal.OnLocalClientReconnected += OnLocalClientReconnected;
            ClientTerminal.OnSessionCompleted += OnSessionCompleted;
            ClientTerminal.OnClientConnected += OnClientConnected;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ClientTerminal.OnLocalClientConnected -= OnLocalClientConnected;
            ClientTerminal.OnSessionCompleted -= OnSessionCompleted;
            ClientTerminal.OnClientConnected -= OnClientConnected;
        }

        private void OnClientConnected(ulong connectionId, string playerId, string playerName)
        {
            if (HasAuthority)
            {
                string players = "";
                string coins = "";
                if (_playersCoins.Count > 0)
                {
                    players = SerializeDictionary(_playersCoins);
                }
                if (_collectedCoinsId.Count > 0)
                {
                    SerializableList list = new SerializableList();
                    list.items = _collectedCoinsId;
                    coins = JsonUtility.ToJson(list, true);
                }
                SendGameDataRpc(_totalCoins, _collectedCoins, players, coins, RpcTarget.Single(connectionId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
        private void SendGameDataRpc(long totalCoins, long collectedCoins, string players, string coins, RpcParams rpcParams = default)
        {
            if(!string.IsNullOrEmpty(players))
            {
                _playersCoins = DeserializeDictionary(players);
            }
            if (!string.IsNullOrEmpty(coins))
            {
                var list = JsonUtility.FromJson<SerializableList>(coins);
                _collectedCoinsId = list.items;
                var all = FindObjectsByType<DemoCoin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (all != null)
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        var net = all[i].GetComponent<NetworkObject>();
                        if (net.NetworkObjectId == 0 || _collectedCoinsId.Contains(net.NetworkObjectId))
                        {
                            all[i].Collected();
                        }
                    }
                }
            }
            _totalCoins = totalCoins;
            _collectedCoins = collectedCoins;
            DemoUI.Instance.UpdateCoinCount(GetCollectedCoins(AuthenticationService.Instance.PlayerId));
        }

        private void OnSessionCompleted()
        {
            long coins = GetCollectedCoins(AuthenticationService.Instance.PlayerId);
            if (BuildConfiguration.Instance.EnableEconomy && coins > 0)
            {
                ClientEconomy.Instance.AddCurrency("GOLD", coins);
            }
        }

        private void Update()
        {
            CheckComplete();
        }

        private void OnLocalClientConnected()
        {
            var point = ClientSpawnPoints.Instance.GetRandomSpawnPoint();
            ClientTerminal.Instance.InstantiatePrefab(playerPrefab, point.position, point.rotation);
        }

        private void OnLocalClientReconnected()
        {

        }

        public void LocalPlayerInstantiated(DemoPlayerController player)
        {
            cameraController.SetTarget(player.GetComponent<DemoCameraTarget>());
            player.SetCameraTransform(cameraController.Camera.transform);
        }

        public void CoinCollected(string id, DemoCoin coin)
        {
            if (string.IsNullOrEmpty(id)) 
            { 
                return;
            }
            if (_playersCoins.ContainsKey(id))
            {
                _playersCoins[id] = _playersCoins[id] + 1;
            }
            else
            {
                _playersCoins[id] = 1;
            }
            _collectedCoins = _collectedCoins + 1;
            ulong coinId = coin.GetComponent<NetworkObject>().NetworkObjectId;
            if (!_collectedCoinsId.Contains(coinId))
            {
                _collectedCoinsId.Add(coinId);
            }
            if (IsClient)
            {
                DemoUI.Instance.UpdateCoinCount(GetCollectedCoins(AuthenticationService.Instance.PlayerId));
                if(SessionManager.Instance.type == SessionManager.ServerType.Persistent && id == AuthenticationService.Instance.PlayerId && BuildConfiguration.Instance.EnableEconomy)
                {
                    ClientEconomy.Instance.AddCurrency("GOLD", 1);
                }
            }
            if(SessionManager.Instance.type == SessionManager.ServerType.Session)
            {
                CheckComplete();
            }
        }

        private void CheckComplete()
        {
            if (_completed || _totalCoins <= 0) 
            {
                return;
            }
            if(!ClientTerminal.Instance.IsOwner)
            {
                return;
            }
            if(_collectedCoins >= _totalCoins)
            {
                _completed = true;
                StartCoroutine(DoComplete());
            }
        }

        private IEnumerator DoComplete()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(1f);
            ClientTerminal.Instance.Complete();
        }

        public long GetCollectedCoins(string id)
        {
            if (_playersCoins.ContainsKey(id))
            {
                return _playersCoins[id];
            }
            return 0;
        }

        [System.Serializable]
        private class SerializableDictionary
        {
            public List<string> keys = new List<string>();
            public List<long> values = new List<long>();
        }

        [System.Serializable]
        private class SerializableList
        {
            public List<ulong> items = new List<ulong>();
        }

        private string SerializeDictionary(Dictionary<string, long> dictionary)
        {
            SerializableDictionary serializableDict = new SerializableDictionary();
            foreach (var kvp in dictionary)
            {
                serializableDict.keys.Add(kvp.Key);
                serializableDict.values.Add(kvp.Value);
            }
            return JsonUtility.ToJson(serializableDict, true);
        }

        private Dictionary<string, long> DeserializeDictionary(string json)
        {
            SerializableDictionary serializableDict = JsonUtility.FromJson<SerializableDictionary>(json);
            Dictionary<string, long> dictionary = new Dictionary<string, long>();
            for (int i = 0; i < serializableDict.keys.Count; i++)
            {
                dictionary[serializableDict.keys[i]] = serializableDict.values[i];
            }
            return dictionary;
        }

    }
}