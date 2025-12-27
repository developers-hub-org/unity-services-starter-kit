using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Exceptions;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientFriendsPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private Button _buttonFriends = null;
        [SerializeField] private Button _buttonRequests = null;
        [SerializeField] private Button _buttonBlocks = null;
        [SerializeField] private Button _buttonAdd = null;
        [SerializeField] private TextMeshProUGUI _textNewRequstsCount = null;
        [SerializeField] private Image _imageNewRequests = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientFriendsItem _friendItemsPrefab = null;
        [SerializeField] private ClientFriendsRequestItem _requestItemsPrefab = null;
        [SerializeField] private ClientFriendsBlockedItem _blockItemsPrefab = null;

        private int _currentTab = 0;

        private void Awake()
        {
            Hide();
        }

        public override void Show()
        {
            base.Show();
            Friends();
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
            _buttonFriends.onClick.AddListener(Friends);
            _buttonRequests.onClick.AddListener(Requests);
            _buttonBlocks.onClick.AddListener(Blocks);
            _buttonAdd.onClick.AddListener(Add);
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

        public void Friends()
        {
            ClearItems();
            _buttonFriends.interactable = false;
            _buttonRequests.interactable = true;
            _buttonBlocks.interactable = true;
            _imageNewRequests.gameObject.SetActive(FriendsService.Instance.IncomingFriendRequests.Count > 0);
            _textNewRequstsCount.text = FriendsService.Instance.IncomingFriendRequests.Count.ToString();
            for (int i = 0; i < FriendsService.Instance.Friends.Count; i++)
            {
                var relationship = FriendsService.Instance.Friends[i];
                var item = Instantiate(_friendItemsPrefab, _itemsContainer);
                item.Initialize(i + 1, relationship);
            }
            _currentTab = 0;
        }

        public void Requests()
        {
            ClearItems();
            _buttonFriends.interactable = true;
            _buttonRequests.interactable = false;
            _buttonBlocks.interactable = true;
            _imageNewRequests.gameObject.SetActive(false);
            for (int i = 0; i < FriendsService.Instance.IncomingFriendRequests.Count; i++)
            {
                var relationship = FriendsService.Instance.IncomingFriendRequests[i];
                var item = Instantiate(_requestItemsPrefab, _itemsContainer);
                item.Initialize(i + 1, relationship);
            }
            _currentTab = 1;
        }

        public void Blocks()
        {
            ClearItems();
            _buttonFriends.interactable = true;
            _buttonRequests.interactable = true;
            _buttonBlocks.interactable = false;
            _imageNewRequests.gameObject.SetActive(FriendsService.Instance.IncomingFriendRequests.Count > 0);
            _textNewRequstsCount.text = FriendsService.Instance.IncomingFriendRequests.Count.ToString();
            for (int i = 0; i < FriendsService.Instance.Blocks.Count; i++)
            {
                var relationship = FriendsService.Instance.Blocks[i];
                var item = Instantiate(_blockItemsPrefab, _itemsContainer);
                item.Initialize(i + 1, relationship);
            }
            _currentTab = 2;
        }

        private void Add()
        {
            QuestionBoxString.Show("Add Friend", "Enter the player name. Note that you need to enter the full name incuding the #???? suffix.", "", "Player Name ...", 3, 20, "Add", "Cancel", TMP_InputField.ContentType.Standard, AddConfirm);
        }

        private async void AddConfirm(bool confirmed, string name)
        {
            if (confirmed)
            {
                var block = ScreenBlock.Show();
                try
                {
                    await FriendsService.Instance.AddFriendByNameAsync(name);
                    MessageBox.MessageBox.Show("Friend Request", "Friend request was sent successfully.", "OK");
                    if (_currentTab == 1)
                    {
                        Requests();
                    }
                }
                catch (FriendsServiceException e)
                {
                    MessageBox.MessageBox.Show("Friend Request", e.Message, "OK");
                    Debug.Log(e.StatusCode.ToString());
                }
                block.Close();
            }
        }

        private void ClearItems()
        {
            var friends = _itemsContainer.GetComponentsInChildren<ClientFriendsItem>(true);
            if (friends != null)
            {
                foreach (var item in friends)
                {
                    Destroy(item.gameObject);
                }
            }
            var requests = _itemsContainer.GetComponentsInChildren<ClientFriendsRequestItem>(true);
            if (requests != null)
            {
                foreach (var item in requests)
                {
                    Destroy(item.gameObject);
                }
            }
            var blocks = _itemsContainer.GetComponentsInChildren<ClientFriendsBlockedItem>(true);
            if (blocks != null)
            {
                foreach (var item in blocks)
                {
                    Destroy(item.gameObject);
                }
            }
        }

    }
}