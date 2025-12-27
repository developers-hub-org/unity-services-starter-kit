using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientLoadingPanel : CanvasPanel
    {

        [SerializeField] private float _minLoadTime = 2f;
        [SerializeField] private TextMeshProUGUI _LoadingProgressText = null;
        [SerializeField] private Image _LoadingBarImage = null;

        public override void Show()
        {
            base.Show();
            if (_LoadingBarImage != null)
            {
                _LoadingBarImage.fillAmount = 0;
            }
            if (_LoadingProgressText != null)
            {
                _LoadingProgressText.text = "";
            }
        }

        public void Show(int scene)
        {
            Show();
            StartCoroutine(LoadSceneAsync(scene));
        }

        private IEnumerator LoadSceneAsync(int scene)
        {
            float loadingTimer = Time.realtimeSinceStartup;
            yield return new WaitForEndOfFrame();
            bool done = false;
            AsyncOperation async = SceneManager.LoadSceneAsync(scene);
            async.allowSceneActivation = false;
            while (!async.isDone && !done)
            {
                float progress = Mathf.Clamp01(async.progress / 0.9f);
                if (_LoadingBarImage != null)
                {
                    _LoadingBarImage.fillAmount = progress;
                }
                if (_LoadingProgressText != null)
                {
                    _LoadingProgressText.text = progress * 100f + "%";
                }
                if (async.progress >= 0.9f)
                {
                    done = true;
                }
                yield return null;
            }
            float remained = _minLoadTime - (Time.realtimeSinceStartup - loadingTimer);
            while (remained > 0)
            {
                remained -= Time.deltaTime;
                yield return null;
            }
            async.allowSceneActivation = true;
        }

    }
}