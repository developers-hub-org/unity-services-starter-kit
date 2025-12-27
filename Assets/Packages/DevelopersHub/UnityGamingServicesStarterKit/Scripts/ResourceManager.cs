using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ResourceManager : MonoBehaviour
    {

        [Space]
        [SerializeField] public Map[] maps = null;

        private static ResourceManager _instance = null; public static ResourceManager Instance { get { GetInstance(); return _instance; } }

        [System.Serializable]
        public class Map
        {
            public string name;
            public int scene;
            public string matchmaker;
        }

        private static void GetInstance()
        {
            if (_instance != null) { return; }
            _instance = FindFirstObjectByType<ResourceManager>();
            if (_instance == null) 
            {
                var prefab = GetPrefab();
                if (prefab != null)
                {
                    _instance = Instantiate(prefab);
                }
            }
            DontDestroyOnLoad(_instance.gameObject);
        }

        private static ResourceManager GetPrefab()
        {
            ResourceManager[] list = Resources.LoadAll<ResourceManager>("");
            if (list != null && list.Length > 0)
            {
                return list[0];
            }
            return null;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

    }
}