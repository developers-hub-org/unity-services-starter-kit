using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLeaderboardsItem : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _textRow = null;
        [SerializeField] private TextMeshProUGUI _textName = null;
        [SerializeField] private TextMeshProUGUI _textID = null;
        [SerializeField] private TextMeshProUGUI _textScore = null;
        [SerializeField] private Button _buttonProfile = null;

        Unity.Services.Leaderboards.Models.LeaderboardEntry _player = null;

        private void Start()
        {
            _buttonProfile.onClick.AddListener(Profile);
        }

        public void Initialize(Unity.Services.Leaderboards.Models.LeaderboardEntry player)
        {
            _player = player;
            _textRow.text = (player.Rank + 1).ToString();
            _textScore.text = player.Score.ToString();
            _textName.text = player.PlayerName;
            _textID.text = player.PlayerId;
        }

        private void Profile()
        {
            CanvasPanel.Get<ClientProfilePanel>().Show(_player.PlayerId, _player.PlayerName);
        }

    }
}