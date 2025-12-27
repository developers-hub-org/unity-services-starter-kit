using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientStartupPanel : CanvasPanel
    {

        [SerializeField] private Button _buttonCreate = null;
        [SerializeField] private Button _buttonLogin = null;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            _buttonCreate.onClick.AddListener(Create);
            _buttonLogin.onClick.AddListener(Login);
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            while (ClientAuthentication.Instance.IsInitialized == false)
            {
                yield return null;
            }
            ClientAuthentication.Instance.Authenticate();
        }

        private void Create()
        {
            ClientAuthentication.Instance.SigninAnonymous();
        }

        private void Login()
        {
            CanvasPanel.Show<ClientLoginPanel>();
        }

    }
}