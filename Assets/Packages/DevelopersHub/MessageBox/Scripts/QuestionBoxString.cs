using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.MessageBox
{
    public class QuestionBoxString : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private TextMeshProUGUI _questionText = null;
        [SerializeField] private TextMeshProUGUI _errorText = null;
        [SerializeField] private TMP_InputField _responseField = null;
        [SerializeField] private Button _confirmButton = null;
        [SerializeField] private Button _cancelButton = null;
        private static List<QuestionBoxString> _instances = new List<QuestionBoxString>();
        public delegate void CallbackDelegate(bool confirmed, string response);
        private CallbackDelegate _callback = null;
        private int _minCharacters = 1;

        private static QuestionBoxString Load()
        {
            QuestionBoxString[] prefabs = Resources.LoadAll<QuestionBoxString>("");
            if (prefabs != null && prefabs.Length > 0)
            {
                return prefabs[0];
            }
            return null;
        }

        public static QuestionBoxString Show(string title, string question, string defaultValue, string placeholder, int minCharacters, int maxCharacters, string confirmButton, string cancelButton, TMP_InputField.ContentType type = TMP_InputField.ContentType.Standard, CallbackDelegate callback = null, QuestionBoxString prefab = null, Canvas canvas = null)
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
                var text = instance._confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = confirmButton;
                }
                text = instance._cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = cancelButton;
                }
                instance._responseField.characterLimit = maxCharacters;
                instance._responseField.contentType = type;
                instance._responseField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholder;
                instance._responseField.text = defaultValue;
                instance._confirmButton.onClick.AddListener(instance.Confirm);
                instance._cancelButton.onClick.AddListener(instance.Cancel);
                instance.transform.SetAsLastSibling();
                instance._callback = callback;
                instance._minCharacters = minCharacters;
                instance._errorText.text = "";
                _instances.Add(instance);
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
                    _instances[i].Cancel();
                }
            }
        }

        public void Confirm()
        {
            string response = _responseField.text;
            if(string.IsNullOrEmpty(response))
            {
                _errorText.text = "Input can not be empty.";
                return;
            }
            if (response.Length < _minCharacters)
            {
                _errorText.text = "Min input size is " + _minCharacters.ToString() + " characters.";
                return;
            }
            _callback?.Invoke(true, response);
            Destroy(gameObject);
        }

        public void Cancel()
        {
            _callback?.Invoke(false, null);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

    }
}