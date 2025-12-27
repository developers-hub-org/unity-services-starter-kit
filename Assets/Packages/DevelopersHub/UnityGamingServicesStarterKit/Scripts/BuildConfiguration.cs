using System.IO;
using System.Linq;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class BuildConfiguration : ScriptableObject
    {

        [SerializeField] private SessionManager.Role _buildTarget = SessionManager.Role.Auto; public SessionManager.Role BuildTarget {  get { return _buildTarget; } }
        [SerializeField] private SessionManager.ServerType _serverType = SessionManager.ServerType.Persistent; public SessionManager.ServerType ServerType { get { return _serverType; } }
        [SerializeField] private SessionManager.SessionType _clientSessionType = SessionManager.SessionType.DistributedAuthority; public SessionManager.SessionType ClientSessionType { get { return _clientSessionType; } }
        [SerializeField] private int _clientMenuScene = 1; public int ClientMenuScene { get { return _clientMenuScene; } }
        [SerializeField] private int _clientLoadWaitTime = 60; public int ClientLoadWaitTime { get { return _clientLoadWaitTime; } }
        [SerializeField] private bool _enableEconomy = true; public bool EnableEconomy { get { return _enableEconomy; } }
        [SerializeField] private bool _enableFriendsService = true; public bool EnableFriendsService { get { return _enableFriendsService; } }
        [SerializeField] private bool _enableSessions = true; public bool EnableSessions { get { return _enableSessions; } }
        [SerializeField] private bool _enablePersistentServers = true; public bool EnablePersistentServers { get { return _enablePersistentServers; } }
        [SerializeField] private bool _enableLeaderboards = true; public bool EnableLeaderboards { get { return _enableLeaderboards; } }
        [SerializeField] private string _leaderboardsId = ""; public string LeaderboardsId { get { return _leaderboardsId; } }

        private static BuildConfiguration _instance = null; public static BuildConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                    if(_instance._buildTarget == SessionManager.Role.Auto) 
                    {
                        switch (Application.platform)
                        {
                            case RuntimePlatform.LinuxServer:
                            case RuntimePlatform.WindowsServer:
                            case RuntimePlatform.OSXServer:
                                _instance._buildTarget = SessionManager.Role.Server;
                                break;
                            default:
                                _instance._buildTarget = SessionManager.Role.Client;
                                break;
                        }
                    }
                }
                return _instance;
            }
        }

        private static BuildConfiguration Load()
        {
            BuildConfiguration[] list = Resources.LoadAll<BuildConfiguration>("");
            if (list != null && list.Length > 0)
            {
                return list[0];
            }
            return null;
        }

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Developers Hub/Gaming Services Starter Kit/Build Configuration")]
        private static void SelectPrefabLibrary()
        {
            Object obj = null;
            BuildConfiguration library = Load();
            if(library == null)
            {
                string path = new string[] { "Assets", "Packages", "DevelopersHub", "UnityGamingServicesStarterKit", "Resources" }.Aggregate(Path.Combine);
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                BuildConfiguration asset = ScriptableObject.CreateInstance<BuildConfiguration>();
                UnityEditor.AssetDatabase.CreateAsset(asset, Path.Combine(path, "BuildConfiguration.asset"));
                UnityEditor.AssetDatabase.SaveAssets();
                obj = asset;
            }
            else
            {
                obj = library;
            }
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = obj;
        }
        #endif

    }
}