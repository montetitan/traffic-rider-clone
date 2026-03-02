using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class PickupNitro : MonoBehaviour
    {
        public float boostMultiplier = 1.25f;
        public float duration = 4f;

        private void Update()
        {
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerBikeController bike = other.GetComponentInParent<PlayerBikeController>();
            if (bike == null) return;
            bike.ApplySpeedBoost(boostMultiplier, duration);
            Destroy(gameObject);
        }
    }
}
