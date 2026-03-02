using System.Collections.Generic;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class PickupSpawner : MonoBehaviour
    {
        public Transform player;
        public float spawnDistance = 80f;
        public float despawnDistance = 20f;
        public float coinInterval = 30f;
        public float nitroInterval = 120f;
        public float[] lanePositions = { -7.5f, -4.5f, -1.5f, 1.5f, 4.5f, 7.5f };

        private readonly List<GameObject> _pickups = new List<GameObject>();
        private float _nextCoinZ;
        private float _nextNitroZ;

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            _nextCoinZ = player.position.z + spawnDistance * 0.5f;
            _nextNitroZ = player.position.z + spawnDistance * 1.5f;
        }

        private void Update()
        {
            if (player == null) return;

            while (_nextCoinZ < player.position.z + spawnDistance)
            {
                SpawnCoinRow(_nextCoinZ);
                _nextCoinZ += coinInterval;
            }

            while (_nextNitroZ < player.position.z + spawnDistance * 2f)
            {
                SpawnNitro(_nextNitroZ);
                _nextNitroZ += nitroInterval;
            }

            Cleanup();
        }

        private void SpawnCoinRow(float z)
        {
            int laneIndex = Random.Range(0, lanePositions.Length);
            Vector3 pos = new Vector3(lanePositions[laneIndex], 0.6f, z);

            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.position = pos;
            coin.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
            {
                color = new Color(1f, 0.82f, 0.1f)
            };
            Collider col = coin.GetComponent<Collider>();
            col.isTrigger = true;
            Rigidbody rb = coin.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            coin.AddComponent<PickupCoin>();
            _pickups.Add(coin);
        }

        private void SpawnNitro(float z)
        {
            int laneIndex = Random.Range(0, lanePositions.Length);
            Vector3 pos = new Vector3(lanePositions[laneIndex], 0.7f, z);

            GameObject nitro = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            nitro.name = "Nitro";
            nitro.transform.position = pos;
            nitro.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
            nitro.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.2f, 0.8f, 1f)
            };
            Collider col = nitro.GetComponent<Collider>();
            col.isTrigger = true;
            Rigidbody rb = nitro.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            nitro.AddComponent<PickupNitro>();
            _pickups.Add(nitro);
        }

        private void Cleanup()
        {
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                GameObject obj = _pickups[i];
                if (obj == null)
                {
                    _pickups.RemoveAt(i);
                    continue;
                }
                if (obj.transform.position.z < player.position.z - despawnDistance)
                {
                    Destroy(obj);
                    _pickups.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                if (_pickups[i] != null)
                {
                    Destroy(_pickups[i]);
                }
            }
            _pickups.Clear();
        }
    }
}
