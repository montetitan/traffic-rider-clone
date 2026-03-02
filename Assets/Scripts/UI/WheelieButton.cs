using TrafficRider.Core;
using TrafficRider.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class WheelieButton : MonoBehaviour
    {
        public float cooldownSeconds = 5f;
        public float boostMultiplier = 1.1f;
        public float boostDuration = 2f;

        private Button _button;
        private Text _label;
        private float _cooldownTimer;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _label = GetComponentInChildren<Text>();
            if (_button != null)
            {
                _button.onClick.AddListener(Activate);
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.unscaledDeltaTime;
                if (_label != null)
                {
                    _label.text = "Wheelie (" + Mathf.CeilToInt(_cooldownTimer) + ")";
                }
                if (_cooldownTimer <= 0f)
                {
                    _cooldownTimer = 0f;
                    if (_button != null) _button.interactable = true;
                    if (_label != null) _label.text = "Wheelie";
                }
            }
        }

        private void Activate()
        {
            if (_cooldownTimer > 0f) return;
            PlayerBikeController player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player != null)
            {
                player.ApplySpeedBoost(boostMultiplier, boostDuration);
            }
            _cooldownTimer = cooldownSeconds;
            if (_button != null) _button.interactable = false;
        }
    }
}
