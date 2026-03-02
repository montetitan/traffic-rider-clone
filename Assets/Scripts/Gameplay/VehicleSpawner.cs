using System.Collections.Generic;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class VehicleSpawner : MonoBehaviour
    {
        public Transform player;
        public float spawnDistance = 120f;
        public float despawnDistance = 30f;
        public float minSpawnInterval = 0.8f;
        public float maxSpawnInterval = 2.4f;
        public float minSpeed = 18f;
        public float maxSpeed = 35f;
        public float[] lanePositions = { -7.5f, -4.5f, -1.5f, 1.5f, 4.5f, 7.5f };
        public float[] laneSpeedMultipliers = { 0.75f, 0.85f, 0.95f, 1.0f, 1.1f, 1.2f };
        public int[] sameDirectionLanes = { 0, 1, 2 };
        public int[] oncomingLanes = { 3, 4, 5 };
        public int minVehiclesAtLowSpeed = 6;
        public int maxVehiclesAtHighSpeed = 14;
        public float lowSpeedKph = 30f;
        public float highSpeedKph = 140f;
        public float oncomingSpawnChance = 0.7f;
        public float oncomingSpeedMultiplier = 1.56f;
        public float noLaneChangeNearPlayer = 18f;
        public float distanceScalePerKm = 0.08f;
        public bool allowOncoming = true;

        private readonly List<TrafficVehicle> _vehicles = new List<TrafficVehicle>();
        private float _nextSpawnTime;
        private PlayerBikeController _playerController;

        public IReadOnlyList<TrafficVehicle> Vehicles => _vehicles;

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            // Push traffic farther ahead to reduce visible pop-in and per-frame spawn hitches.
            spawnDistance = Mathf.Max(spawnDistance, 220f);
            despawnDistance = Mathf.Max(despawnDistance, 60f);
            if (player != null)
            {
                _playerController = player.GetComponent<PlayerBikeController>();
            }
            ConfigureLaneDirections();
            NormalizeLaneGroups();
            _nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        private void ConfigureLaneDirections()
        {
            if (lanePositions == null || lanePositions.Length == 0) return;
            int half = Mathf.Max(1, lanePositions.Length / 2);

            if (!allowOncoming)
            {
                sameDirectionLanes = new int[lanePositions.Length];
                for (int i = 0; i < lanePositions.Length; i++) sameDirectionLanes[i] = i;
                oncomingLanes = new int[0];
                return;
            }

            // Two-way: make left half oncoming and right half same-direction so left-side traffic approaches player.
            oncomingLanes = new int[half];
            for (int i = 0; i < half; i++) oncomingLanes[i] = i;

            int sameCount = lanePositions.Length - half;
            sameDirectionLanes = new int[sameCount];
            for (int i = 0; i < sameCount; i++) sameDirectionLanes[i] = half + i;
        }

        private void Update()
        {
            if (player == null) return;

            float densityT = GetDensityT();
            float distanceKm = player.position.z / 1000f;
            float scale = 1f + distanceKm * distanceScalePerKm;
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, densityT) / scale;
            int maxVehicles = Mathf.RoundToInt(Mathf.Lerp(minVehiclesAtLowSpeed, maxVehiclesAtHighSpeed, densityT) * scale);

            if (Time.time >= _nextSpawnTime && _vehicles.Count < maxVehicles)
            {
                SpawnVehicle();
                _nextSpawnTime = Time.time + Random.Range(interval * 0.8f, interval * 1.2f);
            }

            CleanupVehicles();
        }

        private void SpawnVehicle()
        {
            bool oncoming = allowOncoming && Random.value < oncomingSpawnChance;
            int laneIndex = PickLane(oncoming ? oncomingLanes : (allowOncoming ? sameDirectionLanes : null));
            if (lanePositions == null || lanePositions.Length == 0)
            {
                return;
            }
            laneIndex = Mathf.Clamp(laneIndex, 0, lanePositions.Length - 1);
            float x = lanePositions[laneIndex];
            float z = player.position.z + spawnDistance;

            VehicleFactory.VehicleType type = RandomVehicleType();
            GameObject vehicleObj = VehicleFactory.Create(type);
            vehicleObj.transform.position = new Vector3(x, 0.0f, z);

            TrafficVehicle traffic = vehicleObj.AddComponent<TrafficVehicle>();
            float playerSpeed = _playerController != null ? _playerController.currentSpeed : 0f;
            float playerMax = _playerController != null ? _playerController.maxSpeed : 0f;
            float playerCap = _playerController != null ? (_playerController.maxSpeedCapKph / 3.6f) : playerMax;
            float baseMin = playerMax * 0.4f;
            float baseMax = playerMax * 0.8f;

            if (type == VehicleFactory.VehicleType.Auto)
            {
                baseMin = playerMax * 0.2f;
                baseMax = playerMax * 0.5f;
            }
            else if (type == VehicleFactory.VehicleType.Car)
            {
                baseMin = playerMax * 0.5f;
                baseMax = playerMax * 0.7f;
            }
            else if (type == VehicleFactory.VehicleType.Bus)
            {
                baseMin = playerMax * 0.3f;
                baseMax = playerMax * 0.5f;
            }
            else if (type == VehicleFactory.VehicleType.Truck)
            {
                baseMin = playerMax * 0.3f;
                baseMax = playerMax * 0.5f;
            }
            else if (type == VehicleFactory.VehicleType.Ambulance)
            {
                baseMin = playerMax * 0.35f;
                baseMax = playerMax * 0.75f;
            }

            float scaledBase = Random.Range(baseMin, baseMax);
            traffic.speed = Mathf.Max(scaledBase, playerSpeed * 0.6f);
            traffic.lanePositions = lanePositions;
            traffic.currentLane = laneIndex;
            traffic.laneSpeedMultipliers = laneSpeedMultipliers;
            traffic.player = player;
            traffic.noLaneChangeDistance = noLaneChangeNearPlayer;
            if (type == VehicleFactory.VehicleType.Auto)
            {
                traffic.wobbleScale = 1.8f;
            }
            else if (type == VehicleFactory.VehicleType.Car)
            {
                traffic.wobbleScale = 1.2f;
            }
            else
            {
                traffic.wobbleScale = 0.6f;
            }
            if (oncoming)
            {
                SetLaneBounds(traffic, oncomingLanes);
            }
            else
            {
                SetLaneBounds(traffic, allowOncoming ? sameDirectionLanes : null);
            }

            if (oncoming)
            {
                traffic.speed *= oncomingSpeedMultiplier;
                traffic.isOncoming = true;
                traffic.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                traffic.transform.position = new Vector3(x, 0.0f, z);
            }
            else
            {
                // Same-direction traffic should close distance faster (player overtakes more quickly).
                float laneMul = 1f;
                if (laneSpeedMultipliers != null && laneSpeedMultipliers.Length > 0)
                {
                    laneMul = laneSpeedMultipliers[Mathf.Clamp(laneIndex, 0, laneSpeedMultipliers.Length - 1)];
                }
                float safeLaneMul = Mathf.Max(laneMul, 0.01f);
                float hardFinalCap = Mathf.Max(playerCap * 0.65f, 1f);
                float dynamicFinalCap = Mathf.Max(playerSpeed, playerCap * 0.45f) * 0.72f;
                float finalCap = Mathf.Min(hardFinalCap, dynamicFinalCap);
                float baseCapBeforeLane = finalCap / safeLaneMul;
                traffic.speed = Mathf.Min(traffic.speed, baseCapBeforeLane);
                traffic.speed = Mathf.Max(traffic.speed, playerCap * 0.2f);
                traffic.speed *= 0.5f;
            }

            AlignToGround(vehicleObj);
            EnsureVehicleCollider(vehicleObj);
            _vehicles.Add(traffic);
        }

        private VehicleFactory.VehicleType RandomVehicleType()
        {
            float roll = Random.value;
            if (roll < 0.55f) return VehicleFactory.VehicleType.Car;
            if (roll < 0.7f) return VehicleFactory.VehicleType.Auto;
            if (roll < 0.82f) return VehicleFactory.VehicleType.Bus;
            if (roll < 0.92f) return VehicleFactory.VehicleType.Truck;
            return VehicleFactory.VehicleType.Ambulance;
        }

        private void CleanupVehicles()
        {
            for (int i = _vehicles.Count - 1; i >= 0; i--)
            {
                TrafficVehicle vehicle = _vehicles[i];
                if (vehicle == null)
                {
                    _vehicles.RemoveAt(i);
                    continue;
                }

                if (vehicle.transform.position.z < player.position.z - despawnDistance)
                {
                    Destroy(vehicle.gameObject);
                    _vehicles.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            for (int i = _vehicles.Count - 1; i >= 0; i--)
            {
                if (_vehicles[i] != null)
                {
                    Destroy(_vehicles[i].gameObject);
                }
            }
            _vehicles.Clear();
        }

        private float GetDensityT()
        {
            if (_playerController == null) return 0.2f;
            float kph = _playerController.currentSpeed * 3.6f;
            return Mathf.InverseLerp(lowSpeedKph, highSpeedKph, kph);
        }

        private int PickLane(int[] lanes)
        {
            if (lanePositions == null || lanePositions.Length == 0)
            {
                return 0;
            }
            if (lanes == null || lanes.Length == 0)
            {
                return Random.Range(0, lanePositions.Length);
            }
            int lane = lanes[Random.Range(0, lanes.Length)];
            if (lane < 0 || lane >= lanePositions.Length)
            {
                return Random.Range(0, lanePositions.Length);
            }
            return lane;
        }

        private void NormalizeLaneGroups()
        {
            if (lanePositions == null || lanePositions.Length == 0) return;
            int half = Mathf.Max(1, lanePositions.Length / 2);
            if (sameDirectionLanes == null || sameDirectionLanes.Length == 0)
            {
                sameDirectionLanes = new int[half];
                for (int i = 0; i < half; i++) sameDirectionLanes[i] = i;
            }
            if (oncomingLanes == null || oncomingLanes.Length == 0)
            {
                int count = lanePositions.Length - half;
                oncomingLanes = new int[count];
                for (int i = 0; i < count; i++) oncomingLanes[i] = half + i;
            }
        }

        private void SetLaneBounds(TrafficVehicle vehicle, int[] lanes)
        {
            if (vehicle == null || lanes == null || lanes.Length == 0)
            {
                vehicle.minLaneIndex = 0;
                vehicle.maxLaneIndex = lanePositions.Length - 1;
                return;
            }
            int min = lanes[0];
            int max = lanes[0];
            for (int i = 1; i < lanes.Length; i++)
            {
                min = Mathf.Min(min, lanes[i]);
                max = Mathf.Max(max, lanes[i]);
            }
            vehicle.minLaneIndex = min;
            vehicle.maxLaneIndex = max;
        }

        private void EnsureVehicleCollider(GameObject vehicleObj)
        {
            if (vehicleObj == null) return;

            // Use render bounds to fit a single collider
            Renderer[] renderers = vehicleObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            BoxCollider col = vehicleObj.GetComponent<BoxCollider>();
            if (col == null)
            {
                col = vehicleObj.AddComponent<BoxCollider>();
            }
            Vector3 localCenter = vehicleObj.transform.InverseTransformPoint(bounds.center);
            col.center = localCenter;
            col.size = bounds.size;

            Rigidbody rb = vehicleObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = vehicleObj.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            rb.useGravity = false;
            // Keep rotation fixed but allow X movement for sway/lane-change behavior.
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void AlignToGround(GameObject vehicleObj)
        {
            if (vehicleObj == null) return;
            Renderer[] renderers = vehicleObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            float lift = bounds.min.y;
            if (!Mathf.Approximately(lift, 0f))
            {
                vehicleObj.transform.position = new Vector3(
                    vehicleObj.transform.position.x,
                    vehicleObj.transform.position.y - lift,
                    vehicleObj.transform.position.z);
            }
        }
    }
}
