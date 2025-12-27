using DevelopersHub.MessageBox;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientRuntimeCompletePanel : CanvasPanel
    {

        [SerializeField] private Button _buttonMenu = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientRuntimeCompletePlayerItem _itemsPrefab = null;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonMenu.onClick.AddListener(MainMenu);
        }

        public override void Show()
        {
            CanvasPanel.HideAll();
            base.Show();
            ClearItems();
            foreach (var player in CanvasPanel.Get<ClientRuntimeInitializePanel>()._players)
            {
                var item = Instantiate(_itemsPrefab, _itemsContainer);
                item.Initialize(player.Key, player.Value, null);
            }
            ClientTerminal.Instance.InternalInformComplete();
            SessionManager.Instance.RuntimeEndSession();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void MainMenu()
        {
            var block = ScreenBlock.Show();
            SceneManager.LoadScene(BuildConfiguration.Instance.ClientMenuScene);
        }

        private void ClearItems()
        {
            var items = _itemsContainer.GetComponentsInChildren<ClientRuntimeCompletePlayerItem>(true);
            if (items != null)
            {
                foreach (var item in items)
                {
                    Destroy(item.gameObject);
                }
            }
        }

    }
}