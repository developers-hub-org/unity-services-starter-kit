using System.Collections.Generic;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class CanvasPanel : MonoBehaviour
    {

        [SerializeField] private RectTransform _container = null;
        private bool _isVisible = false; public bool IsVisible { get { return _isVisible; } }
        private static List<CanvasPanel> panels = new List<CanvasPanel>();

        public virtual void Show()
        {
            if (_container != null)
            {
                _container.gameObject.SetActive(true);
                panels.Add(this);
                transform.SetAsLastSibling();
                _isVisible = true;
            }
        }

        public virtual void Hide()
        {
            if (_container != null)
            {
                _container.gameObject.SetActive(false);
                panels.Remove(this);
                _isVisible = false;
            }
        }

        public static void Show<T>(bool hideOthers = true)
        {
            var all = FindObjectsByType<CanvasPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (CanvasPanel panel in all)
            {
                if (panel.GetType() == typeof(T))
                {
                    panel.Show();
                }
                else if (hideOthers)
                {
                    panel.Hide();
                }
            }
        }

        public static T Get<T>() where T : CanvasPanel
        {
            var all = FindObjectsByType<CanvasPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (CanvasPanel panel in all)
            {
                if (panel is T target)
                {
                    return target;
                }
            }
            return default;
        }

        public static void Hide<T>()
        {
            var all = FindObjectsByType<CanvasPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (CanvasPanel panel in all)
            {
                if (panel.GetType() == typeof(T) && panel.IsVisible)
                {
                    panel.Hide();
                }
            }
        }

        public static void HideAll()
        {
            var all = FindObjectsByType<CanvasPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (CanvasPanel panel in all)
            {
                panel.Hide();
            }
        }

    }
}