using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace DevelopersHub.UnityGamingServicesStarterKit.Demo
{
    public class DemoCoin : NetworkBehaviour
    {

        [SerializeField] private GameObject mesh = null;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float bounceHeight = 0.5f;
        [SerializeField] private float bounceSpeed = 2f;

        private Vector3 startPosition;
        private float bounceOffset = 0;
        private bool collected = false;

        void Start()
        {
            startPosition = transform.position;
            bounceOffset = Random.Range(0f, 2f * Mathf.PI);
        }

        void Update()
        {
            RotateEffect();
            BounceEffect();
        }

        private void RotateEffect()
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }

        private void BounceEffect()
        {
            float t = (Mathf.Sin((Time.time + bounceOffset) * bounceSpeed) + 1) * 0.5f;
            float newY = startPosition.y + Mathf.Lerp(0, bounceHeight, t);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !IsSpawned)
            {
                return;
            }
            var player = other.GetComponent<DemoPlayerController>();
            if (player == null)
            {
                return;
            }
            if (HasAuthority)
            {
                collected = true;
                string id = ClientTerminal.Instance.ConnectionToPlayerID(player.OwnerClientId);
                CollectedRpc(id);
                DemoGameManager.Instance.CoinCollected(id, this);
                StartCoroutine(DestroyCoin());
            }
            else
            {
                mesh.SetActive(false);
            }
        }

        [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Reliable)]
        private void CollectedRpc(string id)
        {
            if (collected)
            {
                return;
            }
            collected = true;
            DemoGameManager.Instance.CoinCollected(id, this);
        }

        private IEnumerator DestroyCoin()
        {
            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            mesh.SetActive(!collected);
        }

        public override void OnNetworkDespawn()
        {
            mesh.SetActive(false);
            base.OnNetworkDespawn();
        }

        public void Collected()
        {
            collected = true;
            mesh.SetActive(!collected);
        }

    }
}