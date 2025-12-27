using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopersHub.MessageBox
{
    public class ScreenBlock : MonoBehaviour
    {

        [SerializeField] private Image _blockImage = null;
        private static List<ScreenBlock> _instances = new List<ScreenBlock>();

        private static ScreenBlock Load()
        {
            ScreenBlock[] prefabs = Resources.LoadAll<ScreenBlock>("");
            if (prefabs != null && prefabs.Length > 0)
            {
                return prefabs[0];
            }
            return null;
        }

        public static ScreenBlock Show(ScreenBlock prefab = null, Canvas canvas = null)
        {
            return Show(new Color(0.08f, 0.08f, 0.08f, 0.95f), prefab, canvas);
        }

        public static ScreenBlock Show(Color color, ScreenBlock prefab = null, Canvas canvas = null)
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
                instance._blockImage.color = color;
                instance.transform.SetAsLastSibling();
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
                    _instances[i].Close();
                }
            }
        }

        public void Close()
        {
            try
            {
                Destroy(gameObject);
            }
            catch { }
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

    }
}