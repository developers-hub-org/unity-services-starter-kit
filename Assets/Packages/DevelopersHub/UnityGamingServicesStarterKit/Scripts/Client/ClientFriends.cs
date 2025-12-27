using System;
using System.Threading.Tasks;
using Unity.Services.Friends;
using Unity.Services.Friends.Exceptions;
using Unity.Services.Friends.Notifications;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientFriends : MonoBehaviour
    {

        private static ClientFriends _instance = null; public static ClientFriends Instance { get { GetInstance(); return _instance; } }
        private bool _initialized = false; public bool IsInitialized { get { return _initialized; } }
        private bool _initializing = false; public bool IsInitializing { get { return _initializing; } }
        public delegate void CallbackDelegate(bool response);
        private FriendsEventConnectionState _eventsConnectionState = FriendsEventConnectionState.Unknown;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _instance.RegisterFriendsEventCallbacks();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<ClientFriends>();
            if (_instance == null)
            {
                _instance = new GameObject("ClientFriends").AddComponent<ClientFriends>();
            }
            DontDestroyOnLoad(_instance.gameObject);
            _instance.RegisterFriendsEventCallbacks();
        }

        public async void Initialize(CallbackDelegate callback)
        {
            while (_initializing && !_initialized)
            {
                await Task.Delay(100);
            }
            if (!_initialized)
            {
                try
                {
                    _initializing = true;
                    await FriendsService.Instance.InitializeAsync();
                    _initialized = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                _initializing = false;
            }
            if (callback != null)
            {
                callback.Invoke(_initialized);
            }
        }

        private void RegisterFriendsEventCallbacks()
        {
            try
            {
                FriendsService.Instance.RelationshipAdded += e =>
                {
                    //Debug.Log($"create {e.Relationship} EventReceived");
                };
                FriendsService.Instance.MessageReceived += e =>
                {
                    //Debug.Log("MessageReceived EventReceived");
                };
                FriendsService.Instance.PresenceUpdated += e =>
                {
                    //Debug.Log("PresenceUpdated EventReceived");
                };
                FriendsService.Instance.RelationshipDeleted += e =>
                {
                    //Debug.Log($"Delete {e.Relationship} EventReceived");
                };
                FriendsService.Instance.NotificationsConnectivityChanged += e =>
                {
                    if (_eventsConnectionState == FriendsEventConnectionState.Subscribed && e.State != FriendsEventConnectionState.Subscribed)
                    {
                        // Events disconnected
                    }

                    if (_eventsConnectionState != FriendsEventConnectionState.Subscribed && e.State == FriendsEventConnectionState.Subscribed)
                    {
                        // Events connected
                    }

                    //Debug.Log($"Change of state in notification system from {_eventsConnectionState} to {e.State}");
                    _eventsConnectionState = e.State;
                };
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

    }
}