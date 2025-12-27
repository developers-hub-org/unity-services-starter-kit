using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit
{
    public class ClientSpawnPoints : MonoBehaviour
    {

        [SerializeField] private Transform[] _points = null;
        private bool _initialized = false;
        private int _orderedIndex = -1;
        private static ClientSpawnPoints _instance = null;
        public static ClientSpawnPoints Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ClientSpawnPoints>();
                    if(_instance == null)
                    {
                        int count = 5;
                        _instance = new GameObject("SpawnPoints").AddComponent<ClientSpawnPoints>();
                        _instance._points = new Transform[count];
                        for (int i = 0; i < count; i++)
                        {
                            _instance._points[i] = new GameObject($"SpawnPoint{i.ToString()}").transform;
                            _instance._points[i].position = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f));
                        }
                    }
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Initialize()
        {
            if (_initialized) { return; }
            _initialized = true;
            _orderedIndex = -1;
        }

        private void OnDrawGizmos()
        {
            if (_points != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _points.Length; i++)
                {
                    if (_points[i] != null)
                    {
                        Gizmos.DrawSphere(_points[i].position, 0.1f);
                    }

                }
            }
        }

        public Transform GetSpawnPoint(int index)
        {
            if (index >= 0 && index < _points.Length && _points[index] != null)
            {
                return _points[index];
            }
            return null;
        }

        public Transform GetRandomSpawnPoint()
        {
            return GetSpawnPoint(UnityEngine.Random.Range(0, _points.Length));
        }

        public Transform GetOrderedSpawnPoint()
        {
            _orderedIndex++;
            if (_orderedIndex >= _points.Length)
            {
                _orderedIndex = 0;
            }
            return GetSpawnPoint(_orderedIndex);
        }

    }
}