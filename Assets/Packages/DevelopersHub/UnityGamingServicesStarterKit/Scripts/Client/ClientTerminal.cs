using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientTerminal : NetworkBehaviour
    {

        private static ClientTerminal _instance = null; public static ClientTerminal Instance { get { GetInstance(); return _instance; } }
        private bool _completed = false; public bool Completed { get { return _completed; } }
        private ClientRuntimeCompletePanel _completePanel = null;
        private bool _initialized = false;

        public delegate void CompleteDelegate();
        public static event CompleteDelegate OnSessionCompleted;

        public delegate void InitializeDelegate();
        public static event InitializeDelegate OnLocalClientConnected;

        public delegate void DisconnectDelegate();
        public static event DisconnectDelegate OnLocalClientDisconnected;

        public delegate void ReconnectDelegate();
        public static event ReconnectDelegate OnLocalClientReconnected;

        public delegate void ConnectDelegate(ulong connectionId, string playerId, string playerName);
        public static event ConnectDelegate OnClientConnected;

        public delegate void SpawnDelegate(ref NetworkObject instance, ulong ownerId);
        public static event SpawnDelegate OnPreSpawnPrefab;

        private Dictionary<ulong, List<string>> _playersID = new Dictionary<ulong, List<string>>();

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<ClientTerminal>(FindObjectsInactive.Include);
        }

        private void Awake()
        {
            _instance = this;
            _completePanel = CanvasPanel.Get<ClientRuntimeCompletePanel>();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
            catch { }
            try
            {
                NetworkManager.Singleton.Shutdown();
            }
            catch { }
        }

        private void Update()
        {
            if (IsClient && SessionManager.Instance.type == SessionManager.ServerType.Session && _completed && !_completePanel.IsVisible)
            {
                _completePanel.Show();
            }
        }

        public void Complete()
        {
            if (SessionManager.Instance.type != SessionManager.ServerType.Session)
            {
                Debug.LogWarning("Only the session can finish be completed.");
            }
            else if (HasAuthority)
            {
                CompleteRpc();
                _completed = true;
            }
            else
            {
                Debug.LogWarning("Only the terminal owner can finish the session.");
            }
        }

        [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Reliable)]
        private void CompleteRpc()
        {
            _completed = true;
        }

        public void InstantiatePrefab(NetworkObject prefab, Vector3 position, Quaternion rotation, bool isPlayerObject = false)
        {
            if (prefab != null)
            {
                if (NetworkManager.Singleton.DistributedAuthorityMode)
                {
                    ulong id = NetworkManager.Singleton.LocalClientId;
                    InstantiatePrefabConfirm(id, isPlayerObject, prefab.NetworkObjectId, position, rotation);
                }
                else
                {
                    if (IsServer)
                    {
                        InstantiatePrefabConfirm(0, isPlayerObject, prefab.NetworkObjectId, position, rotation);
                    }
                    else
                    {
                        InstantiatePrefabClientServerRpc(prefab.NetworkObjectId, isPlayerObject, position, rotation);
                    }
                }
            }
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
        public void InstantiatePrefabClientServerRpc(ulong prefabId, bool player, Vector3 position, Quaternion rotation, RpcParams rpcParams = default)
        {
            InstantiatePrefabConfirm(rpcParams.Receive.SenderClientId, player, prefabId, position, rotation);
        }

        private void InstantiatePrefabConfirm(ulong id, bool player, ulong prefabId, Vector3 position, Quaternion rotation)
        {
            var prefab = GetPrefab(prefabId);
            if (prefab != null)
            {
                var instance = Instantiate(prefab, position, rotation).GetComponent<NetworkObject>();
                if (OnPreSpawnPrefab != null)
                {
                    OnPreSpawnPrefab(ref instance, id);
                }
                if (id <= 0)
                {
                    instance.Spawn(true);
                }
                else
                {
                    if (player)
                    {
                        instance.SpawnAsPlayerObject(id, true);
                    }
                    else
                    {
                        instance.SpawnWithOwnership(id, true);
                    }
                }
            }
        }

        private GameObject GetPrefab(ulong id)
        {
            foreach (var list in NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists)
            {
                foreach(var item in list.PrefabList)
                {
                    if (item.Prefab != null && item.Prefab.GetComponent<NetworkObject>().NetworkObjectId == id)
                    {
                        return item.Prefab;
                    }
                }
            }
            return null;
        }

        private void OnClientConnectedCallback(ulong id)
        {
            //Debug.Log("Client connected: " + id);
            if (IsClient && id == NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(InitializeAfterConnect());
            }
        }

        private void OnClientDisconnectCallback(ulong id)
        {
            //Debug.Log("Client disconnected: " + id);
            if (IsClient && id == NetworkManager.Singleton.LocalClientId)
            {
                if (CanvasPanel.Get<ClientRuntimeCompletePanel>()?.IsVisible == true)
                {
                    return;
                }
                CanvasPanel.Show<ClientRuntimeDisconnectPanel>(true);
                if (_initialized)
                {
                    if (OnLocalClientDisconnected != null)
                    {
                        OnLocalClientDisconnected.Invoke();
                    }
                }
            }
        }

        private IEnumerator InitializeAfterConnect()
        {
            yield return new WaitForEndOfFrame();
            float timeout = 10f;
            while (!NetworkManager.Singleton.IsConnectedClient)
            {
                timeout -= Time.deltaTime;
                if (timeout <= 0f)
                {
                    Debug.LogWarning("Timeout while waiting for client connection.");
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            CanvasPanel.Show<ClientRuntimePanel>(true);
            SavePlayerID(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.PlayerName, NetworkManager.Singleton.LocalClientId);
            IntroduceRpc(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.PlayerName);

            if (!_initialized)
            {
                _initialized = true;
                OnLocalClientConnected?.Invoke();
            }
            else
            {
                OnLocalClientReconnected?.Invoke();
            }
        }

        [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
        private void IntroduceRpc(string id, string playerName, RpcParams rpcParams = default)
        {
            SavePlayerID(id, playerName, rpcParams.Receive.SenderClientId);
            if (IsClient)
            {
                IntroducResponseRpc(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.PlayerName, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
            } 
            if(OnClientConnected != null)
            {
                OnClientConnected.Invoke(rpcParams.Receive.SenderClientId, id, playerName);
            }
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
        private void IntroducResponseRpc(string playerId, string playerName, RpcParams rpcParams = default)
        {
            SavePlayerID(playerId, playerName, rpcParams.Receive.SenderClientId);
        }

        private void SavePlayerID(string playerId, string playerName, ulong connectionId)
        {
            foreach (var item in _playersID)
            {
                if (item.Value.Last() == playerId)
                {
                    if (item.Key != connectionId)
                    {
                        item.Value.Add("n");
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (_playersID.TryGetValue(connectionId, out var player))
            {
                if (player.Last() == playerId)
                {
                    player.Add(playerId);
                }
            }
            else
            {
                List<string> ids = new List<string>();
                ids.Add(playerId);
                _playersID.Add(connectionId, ids);
            }
        }

        public ulong PlayerToConnectionID(string playerId)
        {
            foreach (var item in _playersID)
            {
                if (item.Value.Last() == playerId)
                {
                    return item.Key;
                }
            }
            return 0;
        }

        public string ConnectionToPlayerID(ulong connectionId)
        {
            if (_playersID.TryGetValue(connectionId, out var player))
            {
                return player.Last();
            }
            return null;
        }
        
        public void InternalInformComplete()
        {
            if (OnSessionCompleted != null)
            {
                OnSessionCompleted.Invoke();
            }
        }

    }
}