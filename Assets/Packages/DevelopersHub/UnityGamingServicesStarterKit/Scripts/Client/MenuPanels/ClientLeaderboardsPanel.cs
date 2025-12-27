using System.Collections.Generic;
using DevelopersHub.MessageBox;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLeaderboardsPanel : CanvasPanel
    {

        [SerializeField][Range(10, 100)] private int _playersPerPage = 10;
        [SerializeField] private TMP_Dropdown _dropdownPage = null;
        [SerializeField] private Button _buttonNextPage = null;
        [SerializeField] private Button _buttonPrevPage = null;
        [SerializeField] private Button _buttonBack = null;
        [SerializeField] private RectTransform _itemsContainer = null;
        [SerializeField] private ClientLeaderboardsItem _itemsPrefab = null;

        private int _page = 1;
        private int _count = 0;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonBack.onClick.AddListener(Back);
            _buttonNextPage.onClick.AddListener(NextPage);
            _buttonPrevPage.onClick.AddListener(PrevPage);
            _dropdownPage.onValueChanged.AddListener(ChangePage);
        }

        public override void Show()
        {
            base.Show();
            ClearItems();
            Leaderboards(1);
        }

        private void ChangePage(int value)
        {
            int newPage = value + 1;
            if (newPage != _page)
            {
                Leaderboards(newPage);
            }
        }

        private void NextPage()
        {
            if (_page < _count)
            {
                Leaderboards(_page + 1);
            }
        }

        private void PrevPage()
        {
            if (_page > 1)
            {
                Leaderboards(_page - 1);
            }
        }

        public async void Leaderboards(int page)
        {
            ClearItems();
            var block = ScreenBlock.Show();
            var options = new GetScoresOptions();
            options.Limit = _playersPerPage;
            options.Offset = _playersPerPage * (page - 1);
            try
            {
                var scores = await LeaderboardsService.Instance.GetScoresAsync("1", options);
                for (int i = 0; i < scores.Results.Count; i++)
                {
                    var score = scores.Results[i];
                    var item = Instantiate(_itemsPrefab, _itemsContainer);
                    item.Initialize(score);
                }
                _count = Mathf.CeilToInt(scores.Total / (float)_playersPerPage);
                _page = Mathf.Clamp(page, 1, _count);
            }
            catch (LeaderboardsException e)
            {
                _page = 1;
                _count = 1;
                Debug.Log(e.Message);
            }
            UpdateDropdown();
            block.Close();
        }

        private void UpdateDropdown()
        {
            // Update dropdown options
            _dropdownPage.ClearOptions();
            List<string> pageOptions = new List<string>();
            for (int i = 1; i <= _count; i++)
            {
                pageOptions.Add($"{i}");
            }
            _dropdownPage.AddOptions(pageOptions);

            // Set current page (dropdown is 0-based)
            _dropdownPage.SetValueWithoutNotify(_page - 1);

            // Update button interactability
            _buttonPrevPage.interactable = _page > 1;
            _buttonNextPage.interactable = _page < _count;

            // Hide buttons if there's only one page
            _buttonPrevPage.gameObject.SetActive(_count > 1);
            _buttonNextPage.gameObject.SetActive(_count > 1);
            _dropdownPage.gameObject.SetActive(_count > 1);
        }

        private void Back()
        {
            CanvasPanel.Show<ClientMenuPanel>();
        }

        private void ClearItems()
        {
            var items = _itemsContainer.GetComponentsInChildren<ClientLeaderboardsItem>(true);
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