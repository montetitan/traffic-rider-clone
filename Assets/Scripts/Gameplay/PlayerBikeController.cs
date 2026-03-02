using UnityEngine;

namespace TrafficRider.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBikeController : MonoBehaviour
    {
        [Header("Base Stats")]
        public float maxSpeed = 50f;
        public float acceleration = 12f;
        public float brake = 16f;
        public float handling = 6f;

        [Header("Track")]
        public float roadHalfWidth = 9.5f;

        [Header("Runtime")]
        public float currentSpeed;
        public float maxSpeedCapKph = 80f;
        public float slowBrakeKph = 48f;

        private Rigidbody _rb;
        private Transform _visual;
        private float _steerInput;
        private float _throttleInput;
        private float _brakeInput;
        private bool _slowBrakeTouch;
        private float _boostMultiplier = 1f;
        private float _boostTimer;
        private float _rideHeight = 1.0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.position = new Vector3(_rb.position.x, 0.6f, _rb.position.z);

            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = gameObject.AddComponent<CapsuleCollider>();
            }
            capsule.isTrigger = false;
            capsule.radius = 0.35f;
            capsule.height = 1.2f;
            capsule.center = new Vector3(0f, 0.6f, 0f);
            _visual = transform.Find("Visual");
            if (_visual == null)
            {
                GameObject visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(transform);
                visualObj.transform.localPosition = Vector3.zero;
                visualObj.transform.localRotation = Quaternion.identity;
                _visual = visualObj.transform;
                BikeVisual.BuildForBike(_visual, "bike_basic");
            }
            currentSpeed = 0f;
            _rideHeight = 0.6f;
        }

        private void Start()
        {
            RoadSpawner road = FindObjectOfType<RoadSpawner>();
            if (road != null)
            {
                roadHalfWidth = road.roadWidth * 0.5f + 0.35f;
            }
        }

        private void Update()
        {
            ReadInput();
            UpdateBoost();
        }

        private void FixedUpdate()
        {
            float effectiveCapKph = Mathf.Max(1f, maxSpeedCapKph);
            float maxSpeedCap = effectiveCapKph / 3.6f * _boostMultiplier;
            float minSpeedCap = (effectiveCapKph * 0.5f) / 3.6f;
            float targetSpeed = Mathf.Min(maxSpeed, maxSpeedCap) * _throttleInput;
            float accelRate = acceleration;

            if (_slowBrakeTouch)
            {
                float slowTarget = Mathf.Max(1f, slowBrakeKph) / 3.6f;
                targetSpeed = Mathf.Min(targetSpeed, slowTarget);
                accelRate = brake * 0.5f;
            }
            else if (_brakeInput > 0f)
            {
                targetSpeed = 0f;
                accelRate = brake;
            }

            // Do not allow speed below half of total max speed
            targetSpeed = Mathf.Max(targetSpeed, minSpeedCap);

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            Vector3 forwardVelocity = Vector3.forward * currentSpeed;
            Vector3 lateral = Vector3.right * (_steerInput * handling);
            Vector3 velocity = forwardVelocity + lateral;

            _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);

            if (_visual != null)
            {
                float lean = -_steerInput * 20f;
                _visual.localRotation = Quaternion.Euler(0f, 0f, lean);
            }

            Vector3 pos = _rb.position;
            pos.y = _rideHeight;
            pos.x = Mathf.Clamp(pos.x, -roadHalfWidth, roadHalfWidth);
            _rb.position = pos;
        }

        private void ReadInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            _throttleInput = Mathf.Clamp01(vertical);
            _brakeInput = Mathf.Clamp01(-vertical);
            _steerInput = horizontal;

            if (Application.isMobilePlatform)
            {
                Vector3 accel = Input.acceleration;
                _steerInput = Mathf.Clamp(accel.x * 2f, -1f, 1f);

                _slowBrakeTouch = false;
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    _slowBrakeTouch = touch.position.y < Screen.height * 0.25f;
                    if (touch.position.x < Screen.width * 0.4f)
                    {
                        _steerInput = -1f;
                    }
                    else if (touch.position.x > Screen.width * 0.6f)
                    {
                        _steerInput = 1f;
                    }
                }

                _throttleInput = 1f;
                _brakeInput = 0f;
            }
            else
            {
                _slowBrakeTouch = false;
            }
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _boostMultiplier = Mathf.Max(_boostMultiplier, multiplier);
            _boostTimer = Mathf.Max(_boostTimer, duration);
        }

        public void ConfigureSpeedCapsFromBike(float selectedBikeMaxSpeedKph)
        {
            maxSpeedCapKph = Mathf.Max(1f, selectedBikeMaxSpeedKph);
            slowBrakeKph = maxSpeedCapKph * 0.6f;
        }

        private void UpdateBoost()
        {
            if (_boostTimer > 0f)
            {
                _boostTimer -= Time.deltaTime;
                if (_boostTimer <= 0f)
                {
                    _boostMultiplier = 1f;
                    _boostTimer = 0f;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponentInParent<TrafficVehicle>() != null)
            {
                Core.GameManager.Instance?.GameOver();
                return;
            }

            if (collision.collider.name.Contains("GuardRail"))
            {
                Core.GameManager.Instance?.GameOver();
            }
        }
    }
}
