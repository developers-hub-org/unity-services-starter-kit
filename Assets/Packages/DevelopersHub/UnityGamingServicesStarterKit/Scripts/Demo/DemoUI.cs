using TMPro;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textCoin = null;
        private static DemoUI _instance = null; public static DemoUI Instance { get { GetInstance(); return _instance; } }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<DemoUI>(FindObjectsInactive.Include);
        }

        public void UpdateCoinCount(long count)
        {
            _textCoin.text = count.ToString();
        }

    }
}