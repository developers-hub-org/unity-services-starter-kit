using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DevelopersHub.MessageBox
{
    public class QuestionBoxBool : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private TextMeshProUGUI _questionText = null;
        [SerializeField] private Button _positiveButton = null;
        [SerializeField] private Button _negativeButton = null;
        private static List<QuestionBoxBool> _instances = new List<QuestionBoxBool>();
        public delegate void CallbackDelegate(bool response);
        private CallbackDelegate _callback = null;
        private bool _timer = false;
        private float _positiveTimer = 0;
        private TextMeshProUGUI _positiveText = null;
        private string _positiveLabel = "";

        private static QuestionBoxBool Load()
        {
            QuestionBoxBool[] prefabs = Resources.LoadAll<QuestionBoxBool>("");
            if (prefabs != null && prefabs.Length > 0)
            {
                return prefabs[0];
            }
            return null;
        }

        private void Update()
        {
            if (!_timer) { return; }
            _positiveTimer -= Time.unscaledDeltaTime;
            if(_positiveTimer <= 0)
            {
                Positive();
            }
            else
            {
                if (_positiveText != null)
                {
                    _positiveText.text = $"{_positiveLabel} ({Mathf.CeilToInt(_positiveTimer)}s)";
                }
            }
        }

        public static QuestionBoxBool Show(string title, string question, string positiveButton, string negativeButton, CallbackDelegate callback = null, QuestionBoxBool prefab = null, Canvas canvas = null, uint positiveTimer = 0)
        {
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Exclude);
            }
            if (prefab == null)
            {
                prefab = Load();
            }
            if (prefab != null && canvas != null)
            {
                var instance = Instantiate(prefab, canvas.transform);
                if (instance._titleText != null)
                {
                    instance._titleText.text = title;
                }
                if (instance._questionText != null)
                {
                    instance._questionText.text = question;
                }
                instance._positiveText = instance._positiveButton.GetComponentInChildren<TextMeshProUGUI>();
                if (instance._positiveText != null)
                {
                    instance._positiveText.text = positiveButton;
                }
                instance._positiveLabel = positiveButton;
                var text = instance._negativeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = negativeButton;
                }
                instance._positiveButton.onClick.AddListener(instance.Positive);
                instance._negativeButton.onClick.AddListener(instance.Negative);
                instance.transform.SetAsLastSibling();
                instance._callback = callback;
                _instances.Add(instance);
                instance._positiveTimer = positiveTimer;
                instance._timer = positiveTimer > 0;
                return instance;
            }
            return null;
        }

        public static void CloseAll()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (_instances[i] != null)
                {
                    _instances[i].Negative();
                }
            }
        }

        public void Positive()
        {
            _timer = false;
            _callback?.Invoke(true);
            Destroy(gameObject);
        }

        public void Negative()
        {
            _timer = false;
            _callback?.Invoke(false);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

    }
}