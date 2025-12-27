using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Friends.Exceptions;
using Unity.Services.Friends;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientProfilePanel : CanvasPanel
    {

        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textID = null;
        [SerializeField] private Button _buttonClose = null;
        [SerializeField] private Button _buttonAddFriend = null;
        [SerializeField] private Button _buttonRemoveFriend = null;
        [SerializeField] private Button _buttonBlock = null;
        [SerializeField] private Button _buttonUnblock = null;

        private string _id = "";
        private bool _isFriend = false;
        private bool _isBlocked = false;
        private bool _isYou = false;
        private bool _sentRequest = false;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonClose.onClick.AddListener(Close);
            _buttonBlock.onClick.AddListener(Block);
            _buttonUnblock.onClick.AddListener(Unblock);
            _buttonAddFriend.onClick.AddListener(AddFriend);
            _buttonRemoveFriend.onClick.AddListener(RemoveFriend);
        }

        public void Show(Unity.Services.Friends.Models.Member player)
        {
            Show();
            _id = player.Id;
            _textName.text = player.Profile.Name;
            _textID.text = player.Id;
            UpdateButtonStates();
        }

        public void Show(string id, string name)
        {
            Show();
            _id = id;
            _textName.text = name;
            _textID.text = id;
            UpdateButtonStates();
        }

        private void Close()
        {
            Hide();
        }

        private async void AddFriend()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.AddFriendAsync(_id);
                UpdateButtonStates();
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(e.Message);
            }
            block.Close();
        }

        private async void RemoveFriend()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.DeleteFriendAsync(_id);
                UpdateButtonStates();
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(e.Message);
            }
            block.Close();
        }

        private async void Block()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.AddBlockAsync(_id);
                UpdateButtonStates();
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(e.Message);
            }
            block.Close();
        }

        private async void Unblock()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.DeleteBlockAsync(_id);
                UpdateButtonStates();
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(e.Message);
            }
            block.Close();
        }

        private void CheckRelationshipStatus()
        {
            for (int i = 0; i < FriendsService.Instance.Friends.Count; i++)
            {
                if (FriendsService.Instance.Friends[i].Member.Id == _id)
                {
                    _isFriend = true;
                    break;
                }
            }
            for (int i = 0; i < FriendsService.Instance.Blocks.Count; i++)
            {
                if (FriendsService.Instance.Blocks[i].Member.Id == _id)
                {
                    _isBlocked = true;
                    break;
                }
            }
            for (int i = 0; i < FriendsService.Instance.OutgoingFriendRequests.Count; i++)
            {
                if (FriendsService.Instance.OutgoingFriendRequests[i].Member.Id == _id)
                {
                    _sentRequest = true;
                    break;
                }
            }
            _isYou = _id == AuthenticationService.Instance.PlayerId;
        }

        private void UpdateButtonStates()
        {
            CheckRelationshipStatus();

            // Friend buttons
            _buttonAddFriend.gameObject.SetActive(!_isYou && !_isFriend);
            _buttonRemoveFriend.gameObject.SetActive(!_isYou && _isFriend);
            _buttonAddFriend.interactable = !_sentRequest && !_isBlocked;
            _buttonRemoveFriend.interactable = !_isBlocked;

            // Block buttons
            _buttonBlock.gameObject.SetActive(!_isYou && !_isBlocked);
            _buttonUnblock.gameObject.SetActive(!_isYou && _isBlocked);
            _buttonBlock.interactable = !_isFriend;
        }

    }
}