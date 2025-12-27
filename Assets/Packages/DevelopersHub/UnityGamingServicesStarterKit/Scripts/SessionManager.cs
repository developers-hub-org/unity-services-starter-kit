using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class SessionManager : MonoBehaviour
    {

        private static SessionManager _instance = null; public static SessionManager Instance { get { GetInstance(); return _instance; } }
        private Role _role = Role.Auto; public Role role { get { return _role; } }
        private ServerType _serverType = ServerType.None; public ServerType type { get { return _serverType; } }
        private string _ip = "127.0.0.1"; public string ip { get { return _ip; } }
        private ushort _port = 7777; public ushort port { get { return _port; } }
        private ISession _session = null; public ISession session { get { return _session; } }

        public enum Role
        {
            Auto = 0, Server = 1, Client = 2
        }

        public enum ServerType
        {
            None = 0, Persistent = 1, Session = 2
        }

        public enum SessionType
        {
            DistributedAuthority = 0, Relay = 1
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
            _instance = FindFirstObjectByType<SessionManager>();
            if (_instance == null)
            {
                _instance = new GameObject("SessionManager").AddComponent<SessionManager>();
            }
            DontDestroyOnLoad(_instance.gameObject);
        }

        public void InitializeAsClient()
        {
            _role = Role.Client;
        }

        public void InitializeAsServer()
        {
            _role = Role.Server;
        }

        public void InitializePersistent(string ip, ushort port)
        {
            _serverType = ServerType.Persistent;
            _ip = ip;
            _port = port;
        }

        public void InitializeSession(ISession session)
        {
            _serverType = ServerType.Session;
            _session = session;
        }

        public void RefreshSession()
        {
            if(_session == null) { return; }
            foreach (var session in MultiplayerService.Instance.Sessions.Values)
            {
                if(session.Id == _session.Id)
                {
                    _session = session;
                    break;
                }
            }
        }

        public async void RuntimeEndSession()
        {
            if (_session.IsHost)
            {
                try
                {
                    var hostSession = _session.AsHost();
                    var sessionProperty = new SessionProperty("1", VisibilityPropertyOptions.Public);
                    hostSession.SetProperty("ended", sessionProperty);
                    await hostSession.SavePropertiesAsync();
                    await _session.LeaveAsync();
                }
                catch (SessionException e)
                {
                    Debug.Log(e.Message);
                }
            }
            RuntimeLeaveSession();
        }

        public async void RuntimeLeaveSession()
        {
            try
            {
                if(SessionManager.Instance.type == ServerType.Session)
                {
                    await _session.LeaveAsync();
                }
            }
            catch (SessionException e)
            {
                Debug.Log(e.Message);
            }
            try
            {
                NetworkManager.Singleton.Shutdown();
            }
            catch
            {
                
            }
        }

    }
}