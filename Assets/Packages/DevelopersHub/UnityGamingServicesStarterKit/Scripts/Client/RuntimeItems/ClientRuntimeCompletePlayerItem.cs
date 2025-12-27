using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeCompletePlayerItem : MonoBehaviour
    {

        [SerializeField] private Button _buttonProfile = null;
        [SerializeField] private TextMeshProUGUI _textName = null;

        private string _id = null;
        private string _name = null;

        private void Start()
        {
            _buttonProfile.onClick.AddListener(Profile);
        }

        public virtual void Initialize(string id, string name, string data)
        {
            _id = id;
            _name = name;
            _textName.text = name;
        }

        private void Profile()
        {
            CanvasPanel.Get<ClientProfilePanel>().Show(_id, _name);
        }

    }
}