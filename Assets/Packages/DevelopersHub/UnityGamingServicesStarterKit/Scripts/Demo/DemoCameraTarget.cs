using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoCameraTarget : MonoBehaviour
    {

        [SerializeField] private Transform _target = null; public Transform Target { get { return _target; } }

    }
}