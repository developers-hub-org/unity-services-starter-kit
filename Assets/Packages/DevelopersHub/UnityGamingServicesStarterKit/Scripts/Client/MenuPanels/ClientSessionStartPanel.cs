using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientSessionStartPanel : CanvasPanel
    {

        [SerializeField] private uint _autoStartTime = 10;
        [SerializeField] private Button _buttonPlay = null;
        [SerializeField] private Button _buttonCancel = null;
        [SerializeField] private TextMeshProUGUI _textMap = null;
        [SerializeField] private TextMeshProUGUI _textPlayers = null;
        [SerializeField] private TextMeshProUGUI _textTimer = null;
        private Dictionary<ISession, bool> _sessions = new Dictionary<ISession, bool>();
        private ISession _session = null;
        private bool _matchmaking = false;
        private bool _autoStart = false;
        private float _timer = 0;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonPlay.onClick.AddListener(Play);
            _buttonCancel.onClick.AddListener(Cancel);
        }

        private void Update()
        {
            if (!IsVisible || !_autoStart) { return; }
            _timer -= Time.unscaledDeltaTime;
            if (_timer < 0)
            {
                Play();
            }
            else
            {
                UpdateTimerText();
            }
        }

        private void UpdateTimerText()
        {
            if (_autoStart)
            {
                _textTimer.text = "Match Starts in " + Mathf.CeilToInt(_timer).ToString() + " Seconds";
            }
            else
            {
                _textTimer.text = "Waiting for you to Start";
            }
        }

        public void Show(ISession session, bool matchmaking)
        {
            _sessions.TryAdd(session, true);
            if (!IsVisible)
            {
                ShowConfirm();
            }
        }

        private void ShowConfirm()
        {
            _session = null;
            if (_sessions.Count > 0)
            {
                var s = _sessions.FirstOrDefault();
                _session = s.Key;
                _matchmaking |= s.Value;
                _sessions.Remove(s.Key);
            }
            if (_session != null)
            {
                Show();
                _timer = _autoStartTime;
                _autoStart = _autoStartTime > 0;
                UpdateTimerText();
                int mapIndex = 0;
                if (_session.Properties.ContainsKey("map"))
                {
                    int.TryParse(_session.Properties["map"].Value, out mapIndex);
                }
                _textMap.text = ResourceManager.Instance.maps[mapIndex].name;
                _textPlayers.text = "Players: " + _session.Players.Count.ToString();
            }
            else
            {
                Hide();
            }
        }

        private async void Play()
        {
            _autoStart = false;
            int mapIndex = 0;
            if (_session.Properties.ContainsKey("map"))
            {
                int.TryParse(_session.Properties["map"].Value, out mapIndex);
            }
            var block = ScreenBlock.Show();
            try
            {
                var stateProperty = new PlayerProperty(((int)ClientSession.State.StartedSession).ToString(), VisibilityPropertyOptions.Member);
                _session.CurrentPlayer.SetProperty("state", stateProperty);
                await _session.SaveCurrentPlayerDataAsync();
                block.Close();
                SessionManager.Instance.InitializeSession(_session);
                ClientSession.Instance.InitializeSession(_session);
                CanvasPanel.HideAll();
                CanvasPanel.Get<ClientLoadingPanel>().Show(ResourceManager.Instance.maps[mapIndex].scene);
            }
            catch (SessionException e)
            {
                block.Close();
                MessageBox.MessageBox.Show("Match Error", e.Message, "OK");
            }
        }

        private async void Cancel()
        {
            _autoStart = false;
            var block = ScreenBlock.Show();
            try
            {
                bool matcmaked = _session.Properties["matchmaker"].Value == "1";
                bool ready = true;
                if (!matcmaked)
                {
                    ready = _session.CurrentPlayer.Properties["ready"].Value == "1";
                }
                await _session.LeaveAsync();
                CanvasPanel.Get<ClientLobbyListPanel>().LeftSession(_session.Id);
                if (matcmaked || ready)
                {
                    // ToDo: Add penalty
                }
            }
            catch (SessionException e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                block.Close();
                if (_sessions.Count > 0)
                {
                    StartCoroutine(ShowNext());
                }
                else
                {
                    Hide();
                }
            }
        }

        private IEnumerator ShowNext()
        {
            var block = ScreenBlock.Show();
            yield return new WaitForSeconds(1);
            block.Close();
            ShowConfirm();
        }

    }
}