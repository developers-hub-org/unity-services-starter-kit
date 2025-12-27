using TMPro;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoCompletePlayerItem : ClientRuntimeCompletePlayerItem
    {

        [SerializeField] private TextMeshProUGUI _textCoins = null;

        public override void Initialize(string id, string name, string data)
        {
            base.Initialize(id, name, data);
            _textCoins.text = "+" + DemoGameManager.Instance.GetCollectedCoins(id).ToString();
        }

    }
}