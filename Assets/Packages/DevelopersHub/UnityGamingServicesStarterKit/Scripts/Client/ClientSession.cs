using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientSession : MonoBehaviour
    {

        private static ClientSession _instance = null; public static ClientSession Instance { get { GetInstance(); return _instance; } }
        private bool _initialized = false; public bool IsInitialized { get { return _initialized; } }
        private bool _initializing = false; public bool IsInitializing { get { return _initializing; } }
        public delegate void CallbackDelegate(bool response);
        public delegate void SessionsDelegate(QuerySessionsResults sessions);
        private ISession _session = null; public ISession StartedSession { get { return _session; } }
        private Dictionary<string, ClientMigrationHandler> _migrationHandlers = new Dictionary<string, ClientMigrationHandler>();

        public enum State
        {
            None = 0, ReadyToStart = 1, StartedSession = 2, WaitingInRuntimeScene = 3, InGame = 4
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
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
            _instance = FindFirstObjectByType<ClientSession>();
            if (_instance == null)
            {
                _instance = new GameObject("ClientSession").AddComponent<ClientSession>();
            }
            DontDestroyOnLoad(_instance.gameObject);
        }

        public async void Initialize(CallbackDelegate callback)
        {
            while (_initializing && !_initialized)
            {
                await Task.Delay(100);
            }
            if (!_initialized)
            {
                _initializing = true;
                try
                {
                    var sessions = await MultiplayerService.Instance.GetJoinedSessionIdsAsync();
                    /*
                    foreach (var id in sessions)
                    {
                        ReconnectSessionOptions options = new ReconnectSessionOptions();
                        var session = await MultiplayerService.Instance.ReconnectToSessionAsync(id, options);
                        bool started = session.Properties["started"].Value == "1";
                        if (started)
                        {
                            SessionManager.Instance.InitializeSession(session);
                            ClientSession.Instance.InitializeSession(session);
                        }
                    }
                    */
                    _initialized = true;
                    RegisterSessionEvents();
                }
                catch (SessionException e)
                {
                    Debug.Log(e.Message);
                }
                _initializing = false;
            }
            if (callback != null)
            {
                callback.Invoke(_initialized);
            }
        }

        public void Uninitialize()
        {
            _initialized = false;
            UnregisterSessionEvents();
        }

        public void UnregisterMigrationHandler(string id)
        {
            var handler = _migrationHandlers.ContainsKey(id) ? _migrationHandlers[id] : null;
            if(handler != null)
            {
                Destroy(handler.gameObject);
                _migrationHandlers.Remove(id);
            }
        }

        public void RegisterMigrationHandler(string id, ClientMigrationHandler handler)
        {
            _migrationHandlers.Add(id, handler);
        }

        private void OnDestroy()
        {
            UnregisterSessionEvents();
        }

        public async void Query(int count, string continuationToken, SessionsDelegate callback)
        {
            if (callback == null) { return; }
            var options = new QuerySessionsOptions();
            options.Count = count;
            options.ContinuationToken = continuationToken;
            try
            {
                var sessions = await MultiplayerService.Instance.QuerySessionsAsync(options);
                callback.Invoke(sessions);
            }
            catch (System.Exception)
            {
                callback.Invoke(null);
            }
        }

        private void RegisterSessionEvents()
        {
            MultiplayerService.Instance.SessionAdded += SessionAdded;
            MultiplayerService.Instance.SessionRemoved += SessionRemoved;
        }

        private void UnregisterSessionEvents()
        {
            try
            {
                MultiplayerService.Instance.SessionAdded -= SessionAdded;
                MultiplayerService.Instance.SessionRemoved -= SessionRemoved;
            }
            catch { }
        }

        private void SessionAdded(ISession session)
        {
            session.SessionPropertiesChanged += SessionPropertiesChanged;
        }

        private void SessionRemoved(ISession session)
        {
            UnregisterMigrationHandler(session.Id);
            session.SessionPropertiesChanged -= SessionPropertiesChanged;
        }

        private void SessionPropertiesChanged()
        {
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if (session != null && session.Properties["started"].Value == "1")
                {
                    int state = 0;
                    int.TryParse(session.CurrentPlayer.Properties["state"].Value, out state);
                    if (state == (int)State.None || state == (int)State.ReadyToStart)
                    {
                        var panel = CanvasPanel.Get<ClientSessionStartPanel>();
                        if (panel != null && !panel.IsVisible)
                        {
                            panel.Show(session, false);
                        }
                    }
                }
            }
        }

        public void InitializeSession(ISession session)
        {
            _session = session;
        }

        public void RefreshSession()
        {
            if (_session == null) { return; }
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if (session.Id == _session.Id)
                {
                    _session = session;
                    break;
                }
            }
        }

    }
}