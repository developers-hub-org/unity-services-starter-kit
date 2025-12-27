using System.IO;
using System.Linq;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ServerConfiguration : ScriptableObject
    {

        [SerializeField] private string _settingsFolder = "Configuration"; public string settingsFolder { get { return _settingsFolder; } }
        [SerializeField] private string _settingsFile = "configuration.json"; public string serverSettingsFile { get { return _settingsFile; } }

        private static ServerConfiguration _instance = null; public static ServerConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        private static ServerConfiguration Load()
        {
            ServerConfiguration[] list = Resources.LoadAll<ServerConfiguration>("");
            if (list != null && list.Length > 0)
            {
                return list[0];
            }
            return null;
        }
        
        [System.Serializable] public class Configuration
        {
            public ushort port = 7777;
            public int max_players = 200;
            public string password = "";
            public int map_index = 2;
            public static Configuration Get()
            {
                Configuration settings = null;
                var directory = GetDirectory();
                var path = GetPath();
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        settings = JsonUtility.FromJson<Configuration>(json);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex.Message);
                    }
                }
                else
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    settings = new Configuration();
                    string json = JsonUtility.ToJson(settings, true);
                    File.WriteAllText(path, json);
                }
                return settings;
            }
            public static string GetDirectory()
            {
                return Path.Combine(Application.isEditor ? Application.dataPath : Directory.GetCurrentDirectory(), Instance.settingsFolder);
            }
            public static string GetPath()
            {
                return Path.Combine(Path.Combine(Application.isEditor ? Application.dataPath : Directory.GetCurrentDirectory(), Instance.settingsFolder), Instance.serverSettingsFile);
            }
        }

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Developers Hub/Gaming Services Starter Kit/Server Configuration")]
        private static void SelectPrefabLibrary()
        {
            Object obj = null;
            ServerConfiguration library = Load();
            if(library == null)
            {
                string path = new string[] { "Assets", "Packages", "DevelopersHub", "UnityGamingServicesStarterKit", "Resources" }.Aggregate(Path.Combine);
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                ServerConfiguration asset = ScriptableObject.CreateInstance<ServerConfiguration>();
                UnityEditor.AssetDatabase.CreateAsset(asset, Path.Combine(path, "ServerConfiguration.asset"));
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