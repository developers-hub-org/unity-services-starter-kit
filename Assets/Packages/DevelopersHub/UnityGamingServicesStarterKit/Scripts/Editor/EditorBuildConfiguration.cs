using System;
using UnityEditor;

namespace DevelopersHub.UnityGamingServicesStarterKit.Editor
{
    [CustomEditor(typeof(BuildConfiguration))] public class EditorBuildConfiguration : UnityEditor.Editor
    {

        private BuildConfiguration _config = null;
        private SerializedProperty _buildTarget = null;
        private SerializedProperty _serverType = null;
        private SerializedProperty _clientSessionType = null;
        private SerializedProperty _clientMenuScene = null;
        private SerializedProperty _clientLoadWaitTime = null;
        private SerializedProperty _enableEconomy = null;
        private SerializedProperty _enableFriendsService = null;
        private SerializedProperty _enableSessions = null;
        private SerializedProperty _enablePersistentServers = null;
        private SerializedProperty _enableLeaderboards = null;
        private SerializedProperty _leaderboardsId = null;

        private void OnEnable()
        {
            _config = (BuildConfiguration)target;
            _buildTarget = serializedObject.FindProperty("_buildTarget");
            _serverType = serializedObject.FindProperty("_serverType");
            _clientSessionType = serializedObject.FindProperty("_clientSessionType");
            _clientMenuScene = serializedObject.FindProperty("_clientMenuScene");
            _clientLoadWaitTime = serializedObject.FindProperty("_clientLoadWaitTime");
            _enableEconomy = serializedObject.FindProperty("_enableEconomy");
            _enableFriendsService = serializedObject.FindProperty("_enableFriendsService");
            _enableSessions = serializedObject.FindProperty("_enableSessions");
            _enablePersistentServers = serializedObject.FindProperty("_enablePersistentServers");
            _enableLeaderboards = serializedObject.FindProperty("_enableLeaderboards");
            _leaderboardsId = serializedObject.FindProperty("_leaderboardsId");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            _buildTarget.enumValueIndex = (int)(SessionManager.Role)EditorGUILayout.EnumPopup("Build Target", (SessionManager.Role)Enum.GetValues(typeof(SessionManager.Role)).GetValue(_buildTarget.enumValueIndex));
            if(_buildTarget.enumValueIndex == (int)SessionManager.Role.Client)
            {
                _clientSessionType.enumValueIndex = (int)(SessionManager.SessionType)EditorGUILayout.EnumPopup("Session Type", (SessionManager.SessionType)Enum.GetValues(typeof(SessionManager.SessionType)).GetValue(_clientSessionType.enumValueIndex));
                _clientMenuScene.intValue = EditorGUILayout.IntSlider("Menu Scene", _clientMenuScene.intValue, 1, 1000);
                _clientLoadWaitTime.intValue = EditorGUILayout.IntSlider("Load Wait Time", _clientLoadWaitTime.intValue, 10, 300);
            }
            else if (_buildTarget.enumValueIndex == (int)SessionManager.Role.Server)
            {
                _serverType.enumValueIndex = (int)(SessionManager.ServerType)EditorGUILayout.EnumPopup("Type", (SessionManager.ServerType)Enum.GetValues(typeof(SessionManager.ServerType)).GetValue(_serverType.enumValueIndex));
            }
            else if (_buildTarget.enumValueIndex == (int)SessionManager.Role.Auto)
            {
                _clientSessionType.enumValueIndex = (int)(SessionManager.SessionType)EditorGUILayout.EnumPopup("Client Session Type", (SessionManager.SessionType)Enum.GetValues(typeof(SessionManager.SessionType)).GetValue(_clientSessionType.enumValueIndex));
                _clientMenuScene.intValue = EditorGUILayout.IntSlider("Client Menu Scene", _clientMenuScene.intValue, 1, 1000);
                _clientLoadWaitTime.intValue = EditorGUILayout.IntSlider("Client Load Wait Time", _clientLoadWaitTime.intValue, 10, 300);
                _serverType.enumValueIndex = (int)(SessionManager.ServerType)EditorGUILayout.EnumPopup("Server Type", (SessionManager.ServerType)Enum.GetValues(typeof(SessionManager.ServerType)).GetValue(_serverType.enumValueIndex));
            }
            EditorGUILayout.Space();
            _enableEconomy.boolValue = EditorGUILayout.Toggle("Enable Economy", _enableEconomy.boolValue);
            _enableFriendsService.boolValue = EditorGUILayout.Toggle("Enable Friends", _enableFriendsService.boolValue);
            _enableSessions.boolValue = EditorGUILayout.Toggle("Enable Sessions", _enableSessions.boolValue);
            _enablePersistentServers.boolValue = EditorGUILayout.Toggle("Enable Persistent Servers", _enablePersistentServers.boolValue);
            _enableLeaderboards.boolValue = EditorGUILayout.Toggle("Enable Leaderboards", _enableLeaderboards.boolValue);
            if (_enableLeaderboards.boolValue)
            {
                _leaderboardsId.stringValue = EditorGUILayout.TextField("Leaderboards ID", _leaderboardsId.stringValue);
            }
            /*
            if(_buildTarget.enumValueIndex != (int)SessionManager.Role.Server && _clientSessionType.enumValueIndex == (int)SessionManager.SessionType.Relay)
            {
                EditorGUILayout.HelpBox("Client-Server session type is not handled in the code and has been added for later development of this package.", MessageType.Error);
            }
            */
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
        }

    }
}