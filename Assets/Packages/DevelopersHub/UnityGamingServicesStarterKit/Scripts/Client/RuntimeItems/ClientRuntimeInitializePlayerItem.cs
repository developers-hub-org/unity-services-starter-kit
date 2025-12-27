using TMPro;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeInitializePlayerItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textStatus = null;
        private string _id = ""; public string id { get { return _id; } }

        public void Initialize(string id, string name)
        {
            _id = id;
            _textName.text = name;
        }

        public void SetStatus(ClientSession.State status)
        {
            switch (status)
            {
                case ClientSession.State.None:
                case ClientSession.State.ReadyToStart:
                    _textStatus.text = "Unresponsive";
                    _textStatus.color = Color.red;
                    break;
                case ClientSession.State.StartedSession:
                    _textStatus.text = "Connecting";
                    _textStatus.color = Color.yellow;
                    break;
                case ClientSession.State.WaitingInRuntimeScene:
                    _textStatus.text = "Ready";
                    _textStatus.color = Color.green;
                    break;
                case ClientSession.State.InGame:
                    _textStatus.text = "Playing";
                    _textStatus.color = Color.green;
                    break;
                default:
                    _textStatus.text = "Unknown";
                    _textStatus.color = Color.white;
                    break;
            }
        }

    }
}