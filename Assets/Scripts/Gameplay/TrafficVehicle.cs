using System.Collections.Generic;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class TrafficVehicle : MonoBehaviour
    {
        private static readonly List<TrafficVehicle> ActiveVehicles = new List<TrafficVehicle>();

        public float speed = 18f;
        public bool passed;
        public float[] lanePositions;
        public int currentLane;
        public float laneChangeSpeed = 2.5f;
        public float minLaneChangeInterval = 2f;
        public float maxLaneChangeInterval = 5f;
        public float[] laneSpeedMultipliers;
        public bool isOncoming;
        public Transform player;
        public float noLaneChangeDistance = 18f;
        public int minLaneIndex;
        public int maxLaneIndex;
        public float wobbleScale = 1f;
        private int _wobbleSign = 1;
        private Rigidbody _rb;
        private int _previousWobbleSign = 1;
        private Renderer _indicatorRenderer;

        private int _targetLane;
        private float _nextLaneChangeTime;

        private void Start()
        {
            _targetLane = currentLane;
            _wobbleSign = Random.value > 0.5f ? 1 : -1;
            _previousWobbleSign = _wobbleSign;
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            Transform indicator = transform.Find("WobbleIndicator");
            if (indicator != null)
            {
                _indicatorRenderer = indicator.GetComponent<Renderer>();
            }
            ScheduleNextLaneChange();
            ActiveVehicles.Add(this);
        }

        private void OnDestroy()
        {
            ActiveVehicles.Remove(this);
        }

        private void FixedUpdate()
        {
            float dir = isOncoming ? -1f : 1f;
            Vector3 move = Vector3.forward * GetLaneSpeed() * dir * Time.fixedDeltaTime;
            if (_rb != null)
            {
                if (_rb.isKinematic)
                {
                    _rb.MovePosition(_rb.position + move);
                }
                else
                {
                    float dt = Time.fixedDeltaTime;
                    if (dt <= 0f)
                    {
                        return;
                    }
                    float vz = move.z / dt;
                    _rb.velocity = new Vector3(0f, _rb.velocity.y, vz);
                }
            }
            else
            {
                transform.Translate(move, Space.World);
            }

            if (lanePositions != null && lanePositions.Length > 1)
            {
                TryLaneChange();

                int safeTargetLane = Mathf.Clamp(_targetLane, 0, lanePositions.Length - 1);
                float targetX = lanePositions[safeTargetLane];
                Vector3 pos = transform.position;
                pos.x = Mathf.MoveTowards(pos.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
                if (Mathf.Abs(pos.x - targetX) < 0.02f)
                {
                    currentLane = safeTargetLane;
                }
                float minLane = Mathf.Min(lanePositions[0], lanePositions[lanePositions.Length - 1]);
                float maxLane = Mathf.Max(lanePositions[0], lanePositions[lanePositions.Length - 1]);
                pos.x = Mathf.Clamp(pos.x, minLane, maxLane);
                if (_rb != null)
                {
                    if (_rb.isKinematic)
                    {
                        _rb.MovePosition(new Vector3(pos.x, _rb.position.y, _rb.position.z));
                    }
                    else
                    {
                        _rb.position = new Vector3(pos.x, _rb.position.y, _rb.position.z);
                    }
                }
                else
                {
                    transform.position = pos;
                }
            }
        }

        private void Update()
        {
            UpdateLaneChangeIndicator();
        }

        private void UpdateLaneChangeIndicator()
        {
            if (_indicatorRenderer == null) return;
            bool isChangingLane = _targetLane != currentLane;
            if (!isChangingLane)
            {
                _indicatorRenderer.enabled = true;
                Transform indicator = _indicatorRenderer.transform;
                Vector3 pos = indicator.localPosition;
                pos.x = 0f;
                indicator.localPosition = pos;
                return;
            }

            int dir = _targetLane > currentLane ? 1 : -1;
            Transform light = _indicatorRenderer.transform;
            Vector3 lightPos = light.localPosition;
            lightPos.x = 0.28f * dir;
            light.localPosition = lightPos;
            _indicatorRenderer.enabled = (Mathf.Sin(Time.time * 18f) > 0f);
        }

        private void TryLaneChange()
        {
            if (isOncoming) return;
            if (lanePositions == null || lanePositions.Length < 2) return;
            if (Time.time < _nextLaneChangeTime) return;
            if (player != null && Mathf.Abs(player.position.z - transform.position.z) < noLaneChangeDistance)
            {
                ScheduleNextLaneChange();
                return;
            }

            TrafficVehicle blocker = FindFrontBlocker();
            if (blocker == null)
            {
                // Random lane changes keep motion dynamic even without blockers.
                if (Random.value < 0.35f)
                {
                    int[] randomCandidates = { currentLane - 1, currentLane + 1 };
                    for (int i = 0; i < randomCandidates.Length; i++)
                    {
                        int candidate = randomCandidates[i];
                        if (candidate < minLaneIndex || candidate > maxLaneIndex) continue;
                        if (candidate < 0 || candidate >= lanePositions.Length) continue;
                        if (!IsLaneFree(candidate, 8f)) continue;
                        _targetLane = candidate;
                        break;
                    }
                }
                ScheduleNextLaneChange();
                return;
            }

            int bestLane = currentLane;
            float bestGap = -1f;
            int[] candidates = { currentLane - 1, currentLane + 1 };
            for (int i = 0; i < candidates.Length; i++)
            {
                int candidate = candidates[i];
                if (candidate < minLaneIndex || candidate > maxLaneIndex) continue;
                if (candidate < 0 || candidate >= lanePositions.Length) continue;
                if (!IsLaneFree(candidate, 10f)) continue;

                float gap = GetForwardGap(candidate);
                if (gap > bestGap)
                {
                    bestGap = gap;
                    bestLane = candidate;
                }
            }

            if (bestLane != currentLane)
            {
                _targetLane = bestLane;
            }
            ScheduleNextLaneChange();
        }

        private void ScheduleNextLaneChange()
        {
            _nextLaneChangeTime = Time.time + Random.Range(minLaneChangeInterval, maxLaneChangeInterval);
        }

        private TrafficVehicle FindFrontBlocker()
        {
            TrafficVehicle blocker = null;
            float nearest = float.MaxValue;
            for (int i = 0; i < ActiveVehicles.Count; i++)
            {
                TrafficVehicle other = ActiveVehicles[i];
                if (other == null || other == this) continue;
                if (other.isOncoming != isOncoming) continue;
                if (other.currentLane != currentLane) continue;

                float dz = other.transform.position.z - transform.position.z;
                if (dz <= 0f || dz > 18f) continue;
                if (other.GetLaneSpeed() >= GetLaneSpeed() * 0.95f) continue;
                if (dz < nearest)
                {
                    nearest = dz;
                    blocker = other;
                }
            }
            return blocker;
        }

        private bool IsLaneFree(int lane, float zRange)
        {
            for (int i = 0; i < ActiveVehicles.Count; i++)
            {
                TrafficVehicle other = ActiveVehicles[i];
                if (other == null || other == this) continue;
                if (other.isOncoming != isOncoming) continue;
                int otherLane = other._targetLane;
                if (otherLane != lane && other.currentLane != lane) continue;
                if (Mathf.Abs(other.transform.position.z - transform.position.z) < zRange) return false;
            }
            return true;
        }

        private float GetForwardGap(int lane)
        {
            float nearest = 60f;
            for (int i = 0; i < ActiveVehicles.Count; i++)
            {
                TrafficVehicle other = ActiveVehicles[i];
                if (other == null || other == this) continue;
                if (other.isOncoming != isOncoming) continue;
                int otherLane = other._targetLane;
                if (otherLane != lane && other.currentLane != lane) continue;
                float dz = other.transform.position.z - transform.position.z;
                if (dz > 0f && dz < nearest) nearest = dz;
            }
            return nearest;
        }

        private float GetLaneSpeed()
        {
            if (laneSpeedMultipliers == null || laneSpeedMultipliers.Length == 0)
            {
                return speed;
            }
            int laneIndex = Mathf.Clamp(currentLane, 0, laneSpeedMultipliers.Length - 1);
            return speed * laneSpeedMultipliers[laneIndex];
        }
    }
}
