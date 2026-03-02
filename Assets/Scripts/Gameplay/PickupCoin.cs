using UnityEngine;
using TrafficRider.Core;

namespace TrafficRider.Gameplay
{
    public class PickupCoin : MonoBehaviour
    {
        public int value = 5;

        private void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerBikeController>() == null) return;
            GameManager.Instance.AddRunCoins(value);
            Destroy(gameObject);
        }
    }
}
