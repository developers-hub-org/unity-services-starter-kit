using System;
using DevelopersHub.MessageBox;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientAuthentication : MonoBehaviour
    {

        private static ClientAuthentication _instance = null; public static ClientAuthentication Instance { get { GetInstance(); return _instance; } }
        private bool _initialized = false; public bool IsInitialized { get { return _initialized; } }

        private async void Awake()
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
            try
            {
                await UnityServices.InitializeAsync();
                SetupEvents();
                _initialized = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void CreateInstance()
        {
            GetInstance();
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<ClientAuthentication>();
            if (_instance == null)
            {
                _instance = new GameObject("ClientAuthentication").AddComponent<ClientAuthentication>();
            }
            DontDestroyOnLoad(_instance.gameObject);
        }

        public async void Authenticate()
        {
            if (UnityServices.Instance != null && UnityServices.Instance.State == ServicesInitializationState.Initialized)
            {
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        CanvasPanel.Show<ClientMenuPanel>();
                    }
                    else
                    {
                        try
                        {
                            await AuthenticationService.Instance.SignInAnonymouslyAsync();
                        }
                        catch (AuthenticationException e)
                        {
                            MessageBox.MessageBox.Show("Authentication Error", e.Message, "OK");
                        }
                        catch (System.Exception e)
                        {
                            MessageBox.QuestionBoxBool.Show("Authentication Error", e.Message, "Try Again", "Close", AuthenticateFailedResponse);
                        }
                    }
                }
                else
                {
                    CanvasPanel.Show<ClientStartupPanel>();
                }
            }
        }

        private void AuthenticateFailedResponse(bool retry)
        {
            if (retry)
            {
                Authenticate();
            }
        }

        private void SetupEvents()
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                CanvasPanel.Show<ClientMenuPanel>();
                if (BuildConfiguration.Instance.EnableFriendsService)
                {
                    ClientFriends.Instance.Initialize(null);
                }
                if (BuildConfiguration.Instance.EnableSessions)
                {
                    ClientSession.Instance.Initialize(null);
                }
                if (BuildConfiguration.Instance.EnableEconomy)
                {
                    ClientEconomy.Instance.Initialize(null);
                }
            };

            AuthenticationService.Instance.SignInFailed += (err) =>
            {
                CanvasPanel.Show<ClientStartupPanel>();
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                CanvasPanel.Show<ClientStartupPanel>();
                if (BuildConfiguration.Instance.EnableSessions)
                {
                    ClientSession.Instance.Uninitialize();
                }
                if (BuildConfiguration.Instance.EnableEconomy)
                {
                    ClientEconomy.Instance.Uninitialize();
                }
            };

            AuthenticationService.Instance.Expired += () =>
            {
                CanvasPanel.Show<ClientStartupPanel>();
                if (BuildConfiguration.Instance.EnableSessions)
                {
                    ClientSession.Instance.Uninitialize();
                }
                if (BuildConfiguration.Instance.EnableEconomy)
                {
                    ClientEconomy.Instance.Uninitialize();
                }
            };
        }

        public async void SigninAnonymous()
        {
            var block = ScreenBlock.Show();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!string.IsNullOrEmpty(BuildConfiguration.Instance.LeaderboardsId) && AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await LeaderboardsService.Instance.AddPlayerScoreAsync(BuildConfiguration.Instance.LeaderboardsId, 0);
                }
                catch (LeaderboardsException e)
                {
                    Debug.Log(e.Message);
                }
            }
            block.Close();
        }

        public async void SignInWithUsernamePasswordAsync(string username, string password)
        {
            var block = ScreenBlock.Show();
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            }
            catch
            {
                MessageBox.MessageBox.Show("Error", "Username or password is wrong.", "OK");
            }
            block.Close();
        }

    }
}