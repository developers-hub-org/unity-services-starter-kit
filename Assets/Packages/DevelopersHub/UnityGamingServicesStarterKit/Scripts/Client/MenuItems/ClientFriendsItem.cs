using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Exceptions;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientFriendsItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textRow = null;
        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textID = null;
        // [SerializeField] private Image _imageAvatar = null;
        [SerializeField] private Image _imageStatus = null;
        [SerializeField] private Button _buttonBlock = null;
        [SerializeField] private Button _buttonRemove = null;

        private string _id = "";
        private string _relationship = "";

        private void Start()
        {
            _buttonBlock.onClick.AddListener(Block);
            _buttonRemove.onClick.AddListener(Remove);
        }

        public void Initialize(int row, Unity.Services.Friends.Models.Relationship relationship)
        {
            _id = relationship.Member.Id;
            _relationship = relationship.Id;
            _textRow.text = row.ToString();
            _textName.text = relationship.Member.Profile.Name;
            _textID.text = _id.ToString();
            switch (relationship.Member.Presence.Availability)
            {
                case Unity.Services.Friends.Models.Availability.Unknown:
                    _imageStatus.color = Color.white;
                    break;
                case Unity.Services.Friends.Models.Availability.Online:
                    _imageStatus.color = Color.green;
                    break;
                case Unity.Services.Friends.Models.Availability.Busy:
                    _imageStatus.color = Color.yellow;
                    break;
                case Unity.Services.Friends.Models.Availability.Away:
                    _imageStatus.color = Color.blue;
                    break;
                case Unity.Services.Friends.Models.Availability.Invisible:
                    _imageStatus.color = Color.magenta;
                    break;
                case Unity.Services.Friends.Models.Availability.Offline:
                    _imageStatus.color = Color.red;
                    break;
                default:
                    _imageStatus.color = Color.white;
                    break;
            }
        }

        private async void Block()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.AddBlockAsync(_id);
                block.Close();
                Destroy(gameObject);
            }
            catch (FriendsServiceException e)
            {
                block.Close();
                Debug.Log(e.Message);
            }
        }

        private async void Remove()
        {
            var block = ScreenBlock.Show();
            try
            {
                await FriendsService.Instance.DeleteFriendAsync(_id);
                block.Close();
                Destroy(gameObject);
            }
            catch (FriendsServiceException e)
            {
                block.Close();
                Debug.Log(e.Message);
            }

        }

    }
}