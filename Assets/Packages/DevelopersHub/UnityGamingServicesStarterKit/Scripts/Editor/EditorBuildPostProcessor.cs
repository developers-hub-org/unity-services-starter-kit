using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class EditorBuildPostProcessor : MonoBehaviour
    {

        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            // Create settings directory if it doesn't exist
            string directory = Path.Combine(Path.GetDirectoryName(buildPath), ServerConfiguration.Instance.settingsFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create default settings file if it doesn't exist
            string settingsPath = Path.Combine(directory, ServerConfiguration.Instance.serverSettingsFile);
            if (!File.Exists(settingsPath))
            {
                ServerConfiguration.Configuration defaultSettings = new ServerConfiguration.Configuration();
                string json = JsonUtility.ToJson(defaultSettings, true);
                File.WriteAllText(settingsPath, json);
            }
        }

    }
}