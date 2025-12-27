using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class StartupManager : MonoBehaviour
    {

        private SessionManager.Role _role = SessionManager.Role.Client;

        private void Awake()
        {
            _role = BuildConfiguration.Instance.BuildTarget;
            if(_role == SessionManager.Role.Auto)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxServer:
                    case RuntimePlatform.WindowsServer:
                    case RuntimePlatform.OSXServer:
                        _role = SessionManager.Role.Server;
                        break;
                    default:
                        _role = SessionManager.Role.Client;
                        break;
                }
            }
            HandleStartup();
        }

        private void HandleStartup()
        {
            if (_role == SessionManager.Role.Client)
            {
                SessionManager.Instance.InitializeAsClient();
                SceneManager.LoadScene(BuildConfiguration.Instance.ClientMenuScene);
            }
            else if (_role == SessionManager.Role.Server)
            {
                SessionManager.Instance.InitializeAsServer();
                ServerStartup();
            }
        }

        private void ServerStartup()
        {
            if(BuildConfiguration.Instance.ServerType == SessionManager.ServerType.Persistent)
            {
                var config = ServerConfiguration.Configuration.Get();
                SessionManager.Instance.InitializePersistent("127.0.0.1", config.port);
                SceneManager.LoadScene(ResourceManager.Instance.maps[config.map_index].scene);
            }
            else
            {
                // Multiplay Hosting
            }
        }

    }
}