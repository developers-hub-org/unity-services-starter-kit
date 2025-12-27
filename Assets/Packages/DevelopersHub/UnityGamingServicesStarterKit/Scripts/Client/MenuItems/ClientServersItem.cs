using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientServersItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textRow = null;
        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textPing = null;
        [SerializeField] private TextMeshProUGUI _textPlayers = null;
        [SerializeField] private TextMeshProUGUI _textProtection = null;
        [SerializeField] private TextMeshProUGUI _textMap = null;
        [SerializeField] private Button _buttonJoin = null;

        private string _ip = "";
        private int _port = 0;
        private int _scene = 0;

        private void Start()
        {
            _buttonJoin.onClick.AddListener(Join);
        }

        public void Initialize(int row, string name, string ip, int port, string region, int players, int maxPlayers, string password, string mapName, int mapScene)
        {
            _textRow.text = row.ToString();
            _textName.text = name;
            _textPlayers.text = maxPlayers.ToString();
            _textProtection.text = string.IsNullOrEmpty(password) ? "Public" : "Private";
            _textMap.text = mapName;
            _ip = ip;
            _port = port;
            _scene = mapScene;
            //PingServer(ip); // ToDo: This bocks the main thread
        }

        private void Join()
        {
            SessionManager.Instance.InitializePersistent(_ip, (ushort)_port);
            CanvasPanel.HideAll();
            CanvasPanel.Get<ClientLoadingPanel>().Show(_scene);
        }

        private async void PingServer(string ip)
        {
            try
            {
                System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
                PingReply reply = await pingSender.SendPingAsync(ip, 1000);
                if (reply.Status == IPStatus.Success)
                {
                    UpdatePingText(reply.RoundtripTime);
                }
                else
                {
                    UpdatePingText(0);
                }
            }
            catch 
            { 
                UpdatePingText(0);
            }
        }

        private void UpdatePingText(long ping)
        {
            Color color = Color.white;
            if (ping >= 200)
            {
                color = Color.red;
            }
            else if (ping >= 100)
            {
                color = Color.yellow;
            }
            else if (ping > 0)
            {
                color = Color.green;
            }
            _textPing.text = ping.ToString();
            _textPing.color = color;
        }

    }
}