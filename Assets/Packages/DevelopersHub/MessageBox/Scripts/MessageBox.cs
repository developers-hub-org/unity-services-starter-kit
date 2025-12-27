using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DevelopersHub.MessageBox
{
    public class MessageBox : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private TextMeshProUGUI _messageText = null;
        [SerializeField] private Button _closeButton = null;
        private static List<MessageBox> _instances = new List<MessageBox>();
        public delegate void CallbackDelegate();
        private CallbackDelegate _callback = null;

        private static MessageBox Load()
        {
            MessageBox[] prefabs = Resources.LoadAll<MessageBox>("");
            if (prefabs != null && prefabs.Length > 0)
            {
                return prefabs[0];
            }
            return null;
        }

        public static MessageBox Show(string title, string message, string button, CallbackDelegate onClose = null, MessageBox prefab = null, Canvas canvas = null)
        {
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Exclude);
            }
            if(prefab == null)
            {
                prefab = Load();
            }
            if (prefab != null && canvas != null)
            {
                var instance = Instantiate(prefab, canvas.transform);
                if(instance._titleText != null)
                {
                    instance._titleText.text = title;
                }
                if (instance._messageText != null)
                {
                    instance._messageText.text = message;
                }
                var text = instance._closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if(text != null)
                {
                    text.text = button;
                }
                instance._closeButton.onClick.AddListener(instance.Close);
                instance.transform.SetAsLastSibling();
                instance._callback = onClose;
                _instances.Add(instance);
                return instance;
            }
            return null;
        }

        public static void CloseAll()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if( _instances[i] != null)
                {
                    _instances[i].Close();
                }
            }
        }

        public void Close()
        {
            _callback?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

    }
}