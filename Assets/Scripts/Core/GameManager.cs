using System.Collections;
using TrafficRider.Data;
using TrafficRider.Gameplay;
using TrafficRider.Systems;
using TrafficRider.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

namespace TrafficRider.Core
{
    public enum GameMode
    {
        Endless,
        Missions
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Runtime")]
        public GameMode mode = GameMode.Endless;

        public GameConfig Config { get; private set; }
        public SaveData Save { get; private set; }
        public UpgradeSystem UpgradeSystem { get; private set; }
        public MissionSystem MissionSystem { get; private set; }
        public BikeSelectionSystem BikeSelectionSystem { get; private set; }

        public PlayerBikeController Player { get; private set; }
        public RoadSpawner RoadSpawner { get; private set; }
        public VehicleSpawner VehicleSpawner { get; private set; }
        public PickupSpawner PickupSpawner { get; private set; }
        public BackgroundScroller BackgroundScroller { get; private set; }
        public UIController UIController { get; private set; }

        private float _distanceTraveled;
        private int _overtakes;
        private float _lastPlayerZ;
        private bool _initialized;
        private float _saveTimer;
        private bool _isGameOver;
        private bool _paused;
        private int _runCoins;
        private CrashEffects _crashEffects;
        private CameraEffects _cameraEffects;
        private Coroutine _crashRoutine;
        private bool _menuOnlyInitialized;
        private bool _missionCompletedThisRun;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _crashEffects = gameObject.AddComponent<CrashEffects>();
            ApplyRuntimeQuality();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            if (!_initialized)
            {
                PlayerBikeController player = FindObjectOfType<PlayerBikeController>();
                RoadSpawner road = FindObjectOfType<RoadSpawner>();
                VehicleSpawner vehicles = FindObjectOfType<VehicleSpawner>();
                PickupSpawner pickups = FindObjectOfType<PickupSpawner>();
                BackgroundScroller background = FindObjectOfType<BackgroundScroller>();
                UIController ui = FindObjectOfType<UIController>();
                if (player != null && road != null && vehicles != null && ui != null)
                {
                    Initialize(player, road, vehicles, ui);
                    PickupSpawner = pickups;
                    BackgroundScroller = background;
                    if (PickupSpawner != null) PickupSpawner.Initialize(player.transform);
                    if (BackgroundScroller != null) BackgroundScroller.Initialize(player.transform);
                }
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsMenuScene(scene))
            {
                return;
            }

            EnsureGameplaySceneObjects();

            if (Player != null && RoadSpawner != null && VehicleSpawner != null && UIController != null)
            {
                Initialize(Player, RoadSpawner, VehicleSpawner, UIController);
            }
        }

        public void Initialize(PlayerBikeController player, RoadSpawner roadSpawner, VehicleSpawner vehicleSpawner, UIController ui)
        {
            if (_initialized && !_menuOnlyInitialized && Player != null && RoadSpawner != null && VehicleSpawner != null && UIController != null) return;
            _initialized = true;
            _menuOnlyInitialized = false;

            Player = player;
            RoadSpawner = roadSpawner;
            VehicleSpawner = vehicleSpawner;
            UIController = ui;
            if (PickupSpawner == null) PickupSpawner = FindObjectOfType<PickupSpawner>();
            if (BackgroundScroller == null) BackgroundScroller = FindObjectOfType<BackgroundScroller>();

            if (Config != null && Save != null && BikeSelectionSystem != null && UpgradeSystem != null && MissionSystem != null)
            {
                ApplyBikeAndUpgrades();
                RoadSpawner.Initialize(Player.transform);
                VehicleSpawner.Initialize(Player.transform);
                ApplyTrafficMode();
                ApplyOrientation();
                ApplyBackground();
                if (PickupSpawner != null) PickupSpawner.Initialize(Player.transform);
                if (BackgroundScroller != null) BackgroundScroller.Initialize(Player.transform);
                _lastPlayerZ = Player.transform.position.z;
                CacheCameraEffects();
            }
            else
            {
                StartCoroutine(LoadConfigAndStart());
            }
        }

        public void InitializeMenuOnly()
        {
            if (_initialized) return;
            _initialized = true;
            _menuOnlyInitialized = true;
            StartCoroutine(LoadConfigForMenu());
        }

        private IEnumerator LoadConfigAndStart()
        {
            ConfigLoader loader = gameObject.AddComponent<ConfigLoader>();
            yield return loader.LoadConfig(config => Config = config);
            SafeDestroy(loader);

            Save = SaveSystem.Load(Config);
            UpgradeSystem = new UpgradeSystem(Config, Save);
            MissionSystem = new MissionSystem(Config, Save);
            BikeSelectionSystem = new BikeSelectionSystem(Config, Save);

            EnsureGameplaySystems();
            ApplyModeFromSave();
            ApplyBikeAndUpgrades();
            RoadSpawner.Initialize(Player.transform);
            VehicleSpawner.Initialize(Player.transform);
            ApplyTrafficMode();
            ApplyOrientation();
            ApplyBackground();
            ApplyQualityFromSave();
            if (PickupSpawner != null) PickupSpawner.Initialize(Player.transform);
            if (BackgroundScroller != null) BackgroundScroller.Initialize(Player.transform);
            _lastPlayerZ = Player.transform.position.z;
            CacheCameraEffects();
        }

        private IEnumerator LoadConfigForMenu()
        {
            ConfigLoader loader = gameObject.AddComponent<ConfigLoader>();
            yield return loader.LoadConfig(config => Config = config);
            SafeDestroy(loader);

            Save = SaveSystem.Load(Config);
            UpgradeSystem = new UpgradeSystem(Config, Save);
            MissionSystem = new MissionSystem(Config, Save);
            BikeSelectionSystem = new BikeSelectionSystem(Config, Save);
            ApplyModeFromSave();
            ApplyOrientation();
            ApplyBackground();
            ApplyQualityFromSave();
            RefreshMenuUIs();
        }

        private void RefreshMenuUIs()
        {
            BikeMenuUIController bikeMenu = FindObjectOfType<BikeMenuUIController>();
            if (bikeMenu != null)
            {
                bikeMenu.Refresh();
            }
            MissionMenuUIController missionMenu = FindObjectOfType<MissionMenuUIController>();
            if (missionMenu != null)
            {
                missionMenu.Refresh();
            }
        }

        private static bool IsMenuScene(Scene scene)
        {
            if (scene.name.ToLowerInvariant().Contains("menu")) return true;
            return Object.FindObjectOfType<MenuSceneMarker>() != null;
        }

        private void EnsureGameplaySceneObjects()
        {
            if (Player == null)
            {
                Player = FindObjectOfType<PlayerBikeController>();
            }
            if (Player == null)
            {
                GameObject playerObj = new GameObject("PlayerBike");
                playerObj.transform.position = new Vector3(0f, 0.6f, 0f);

                CapsuleCollider collider = playerObj.AddComponent<CapsuleCollider>();
                collider.radius = 0.35f;
                collider.height = 1.2f;
                collider.center = new Vector3(0f, 0.6f, 0f);

                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(playerObj.transform);
                visual.transform.localPosition = Vector3.zero;

                Rigidbody rb = playerObj.AddComponent<Rigidbody>();
                rb.mass = 120f;
                rb.drag = 0.5f;
                Player = playerObj.AddComponent<PlayerBikeController>();
                BikeVisual.BuildForBike(visual.transform, "bike_basic");

                if (Camera.main == null)
                {
                    GameObject cameraObj = new GameObject("MainCamera");
                    Camera cam = cameraObj.AddComponent<Camera>();
                    cam.clearFlags = CameraClearFlags.Skybox;
                    cam.nearClipPlane = 0.3f;
                    cam.farClipPlane = 300f;
                    cameraObj.tag = "MainCamera";
                    cameraObj.transform.position = new Vector3(0f, 4.5f, -8f);
                    cameraObj.transform.SetParent(playerObj.transform);
                    cameraObj.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                    cameraObj.AddComponent<CameraEffects>();
                }
            }

            EnsureGameplaySystems();
        }

        private void EnsureGameplaySystems()
        {
            if (Player == null)
            {
                Player = FindObjectOfType<PlayerBikeController>();
            }
            if (Player == null)
            {
                return;
            }

            if (RoadSpawner == null)
            {
                RoadSpawner = FindObjectOfType<RoadSpawner>();
                if (RoadSpawner == null)
                {
                    GameObject roadObj = new GameObject("RoadSpawner");
                    RoadSpawner = roadObj.AddComponent<RoadSpawner>();
                }
            }
            if (VehicleSpawner == null)
            {
                VehicleSpawner = FindObjectOfType<VehicleSpawner>();
                if (VehicleSpawner == null)
                {
                    GameObject trafficObj = new GameObject("VehicleSpawner");
                    VehicleSpawner = trafficObj.AddComponent<VehicleSpawner>();
                }
            }
            if (PickupSpawner == null)
            {
                PickupSpawner = FindObjectOfType<PickupSpawner>();
                if (PickupSpawner == null)
                {
                    GameObject pickupObj = new GameObject("PickupSpawner");
                    PickupSpawner = pickupObj.AddComponent<PickupSpawner>();
                }
            }
            if (BackgroundScroller == null)
            {
                BackgroundScroller = FindObjectOfType<BackgroundScroller>();
                if (BackgroundScroller == null)
                {
                    GameObject backgroundObj = new GameObject("BackgroundScroller");
                    BackgroundScroller = backgroundObj.AddComponent<BackgroundScroller>();
                }
            }
            if (UIController == null)
            {
                UIController = FindObjectOfType<UIController>();
                if (UIController == null)
                {
                    GameObject uiObj = new GameObject("UIController");
                    UIController = uiObj.AddComponent<UIController>();
                }
            }
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        private void Update()
        {
            if (Player == null || Config == null || Save == null || _isGameOver || _paused) return;
            TrackDistanceAndOvertakes();
            CheckFallbackCollisions();
            UpdateUI();
        }

        private void CheckFallbackCollisions()
        {
            if (Player == null) return;
            Collider playerCol = Player.GetComponent<Collider>();
            if (playerCol == null) return;

            CheckRailCollisionByBounds(playerCol);
            if (_isGameOver) return;

            if (VehicleSpawner == null) return;

            for (int i = 0; i < VehicleSpawner.Vehicles.Count; i++)
            {
                TrafficVehicle traffic = VehicleSpawner.Vehicles[i];
                if (traffic == null) continue;
                Collider trafficCol = traffic.GetComponent<Collider>();
                if (trafficCol == null) continue;

                // Physics callbacks can occasionally miss at high relative speed on some Android devices.
                // Bounds intersection provides a deterministic fallback for game-over collision.
                if (playerCol.bounds.Intersects(trafficCol.bounds))
                {
                    GameOver();
                    return;
                }
            }
        }

        private void CheckRailCollisionByBounds(Collider playerCol)
        {
            if (RoadSpawner == null) return;

            float halfRoad = RoadSpawner.roadWidth * 0.5f;
            float railInnerX = halfRoad + 0.22f; // close to rail inner face (rails are spawned at +/-halfRoad+0.25)
            Bounds pb = playerCol.bounds;
            float playerOuterX = Mathf.Abs(pb.center.x) + pb.extents.x;

            if (playerOuterX >= railInnerX)
            {
                GameOver();
            }
        }

        private void TrackDistanceAndOvertakes()
        {
            Rigidbody rb = Player.GetComponent<Rigidbody>();
            float currentZ = rb != null ? rb.position.z : Player.transform.position.z;
            float delta = currentZ - _lastPlayerZ;
            if (delta > 0f)
            {
                _distanceTraveled += delta;
                float km = _distanceTraveled / 1000f;
                if (km > Save.topScoreKm)
                {
                    Save.topScoreKm = km;
                }
                int coinsEarned = Mathf.FloorToInt(delta * (Config.economy.coinsPerMeter));
                AddRunCoins(coinsEarned);
                _lastPlayerZ = currentZ;
            }

            if (VehicleSpawner != null)
            {
                foreach (TrafficVehicle traffic in VehicleSpawner.Vehicles)
                {
                    if (traffic == null || traffic.passed) continue;
                    if (traffic.transform.position.z < currentZ)
                    {
                        traffic.passed = true;
                        _overtakes += 1;
                        AddRunCoins(Config.economy.coinsPerOvertake);
                        if (mode == GameMode.Missions)
                        {
                            _runCoins += MissionSystem.AddProgress("overtake", 1, true, Save.selectedMissionId);
                            CheckMissionCompletion();
                        }
                    }
                }
            }

            if (mode == GameMode.Missions && delta > 0f)
            {
                _runCoins += MissionSystem.AddProgress("distance", Mathf.FloorToInt(delta), true, Save.selectedMissionId);
                CheckMissionCompletion();
            }

            _saveTimer += Time.deltaTime;
            if (_saveTimer >= 1f)
            {
                _saveTimer = 0f;
                SaveSystem.Save(Save);
            }
        }

        private void UpdateUI()
        {
            if (Save == null) return;
            if (UIController == null)
            {
                UIController = FindObjectOfType<UIController>();
                if (UIController == null && Player != null)
                {
                    GameObject uiObj = new GameObject("UIController");
                    UIController = uiObj.AddComponent<UIController>();
                }
            }
            if (UIController == null) return;
            if (!UIController.gameObject.activeSelf)
            {
                UIController.gameObject.SetActive(true);
            }
            UIController.SetSpeed(Player.currentSpeed * 3.6f);
            UIController.SetDistance(_distanceTraveled / 1000f);
            UIController.SetCoins(_runCoins);
            UIController.SetMode(mode.ToString());
            UIController.SetOvertakes(_overtakes);
            if (mode == GameMode.Missions)
            {
                UIController.SetMissions(MissionSystem);
            }
            else
            {
                UIController.SetMissions(null);
            }
            UIController.SetRunCoins(_runCoins);
            UIController.SetTopScore(Save.topScoreKm, Save.googlePlayId);
            UIController.SetTrafficDebug(BuildTrafficDebug());
        }

        private string BuildTrafficDebug()
        {
            if (VehicleSpawner == null)
            {
                return "Traffic: (no spawner)";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Traffic: ");
            sb.Append("Vehicles ");
            sb.Append(VehicleSpawner.Vehicles != null ? VehicleSpawner.Vehicles.Count : 0);
            sb.Append(" | Spawn ");
            sb.Append(VehicleSpawner.minSpawnInterval.ToString("F1"));
            sb.Append("-");
            sb.Append(VehicleSpawner.maxSpawnInterval.ToString("F1"));
            sb.Append("s | Speed ");
            sb.Append(VehicleSpawner.minSpeed.ToString("F0"));
            sb.Append("-");
            sb.Append(VehicleSpawner.maxSpeed.ToString("F0"));
            sb.Append(" m/s");
            sb.Append(" | Oncoming ");
            sb.Append((VehicleSpawner.oncomingSpawnChance * 100f).ToString("F0"));
            sb.Append("% x");
            sb.Append(VehicleSpawner.oncomingSpeedMultiplier.ToString("F2"));
            sb.Append(" | Wobble A/C/B ");
            sb.Append("1.8/1.2/0.6");
            return sb.ToString();
        }

        public void SetMode(GameMode newMode)
        {
            mode = newMode;
            _distanceTraveled = 0f;
            _overtakes = 0;
            if (Save != null)
            {
                Save.selectedMode = mode == GameMode.Missions ? "missions" : "endless";
            }
            SaveSystem.Save(Save);
        }

        private void ApplyModeFromSave()
        {
            if (Save == null) return;
            if (Save.selectedMode == "missions")
            {
                mode = GameMode.Missions;
            }
            else
            {
                mode = GameMode.Endless;
            }
        }

        public void RefreshBikeStats()
        {
            ApplyBikeAndUpgrades();
            SaveSystem.Save(Save);
        }

        public void SetTrafficMode(bool twoWay)
        {
            if (Save == null) return;
            Save.twoWayTraffic = twoWay;
            ApplyTrafficMode();
            ApplyBackground();
            if (VehicleSpawner != null)
            {
                VehicleSpawner.ClearAll();
                VehicleSpawner.Initialize(Player.transform);
            }
            SaveSystem.Save(Save);
        }

        public void SetOrientation(string mode)
        {
            if (Save == null) return;
            Save.orientation = mode;
            ApplyOrientation();
            SaveSystem.Save(Save);
        }

        public void SetBackground(string theme)
        {
            if (Save == null) return;
            Save.background = theme;
            ApplyBackground();
            MenuBackdrop backdrop = FindObjectOfType<MenuBackdrop>();
            if (backdrop != null)
            {
                backdrop.SetTheme(theme);
            }
            SaveSystem.Save(Save);
        }

        public void SetQuality(string quality)
        {
            if (Save == null) return;
            Save.quality = quality;
            ApplyQualityFromSave();
            SaveSystem.Save(Save);
        }

        public void GameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            CommitRunCoins();
            UIController?.SetGameOverTitle("Game Over");

            if (Player != null) Player.enabled = false;
            if (Player != null)
            {
                Rigidbody rb = Player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            if (RoadSpawner != null) RoadSpawner.enabled = false;
            if (VehicleSpawner != null) VehicleSpawner.enabled = false;

            if (VehicleSpawner != null)
            {
                foreach (TrafficVehicle traffic in VehicleSpawner.Vehicles)
                {
                    if (traffic != null) traffic.enabled = false;
                }
            }

            if (Player != null)
            {
                _crashEffects?.Play(Player.transform.position + Vector3.up * 0.3f);
            }

            if (_crashRoutine == null)
            {
                _crashRoutine = StartCoroutine(CrashSequence());
            }
        }

        public void Restart()
        {
            _isGameOver = false;
            _runCoins = 0;
            _missionCompletedThisRun = false;
            ResetCoinMissionsForRun();
            Time.timeScale = 1f;
            _paused = false;
            UIController?.ShowGameOver(false);

            if (!TryResetInPlace())
            {
                Scene active = SceneManager.GetActiveScene();
                if (active.buildIndex >= 0)
                {
                    SceneManager.LoadScene(active.buildIndex);
                }
                else
                {
                    SceneManager.LoadScene(active.name);
                }
            }
        }

        private void ApplyBikeAndUpgrades()
        {
            BikeConfig bike = BikeSelectionSystem.GetSelectedBike();
            if (bike == null) return;

            float effectiveMaxSpeed = bike.maxSpeed + UpgradeSystem.GetModifier("max_speed");
            Player.maxSpeed = effectiveMaxSpeed;
            Player.acceleration = bike.acceleration + UpgradeSystem.GetModifier("acceleration");
            Player.handling = bike.handling + UpgradeSystem.GetModifier("handling");
            Player.brake = bike.brake + UpgradeSystem.GetModifier("brake");
            Player.ConfigureSpeedCapsFromBike(effectiveMaxSpeed);
            Transform visual = Player.transform.Find("Visual");
            if (visual != null)
            {
                BikeVisual.BuildForBike(visual, bike.id);
            }
        }

        private bool TryResetInPlace()
        {
            if (Player == null || RoadSpawner == null || VehicleSpawner == null) return false;

            Rigidbody rb = Player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = new Vector3(0f, 0.6f, 0f);
            }
            Player.transform.position = new Vector3(0f, 0.6f, 0f);
            Player.enabled = true;
            Player.currentSpeed = 0f;

            _distanceTraveled = 0f;
            _overtakes = 0;
            _lastPlayerZ = Player.transform.position.z;
            _missionCompletedThisRun = false;
            ResetCoinMissionsForRun();

            RoadSpawner.enabled = true;
            VehicleSpawner.enabled = true;
            RoadSpawner.ClearAll();
            VehicleSpawner.ClearAll();
            RoadSpawner.Initialize(Player.transform);
            VehicleSpawner.Initialize(Player.transform);
            ApplyTrafficMode();
            if (PickupSpawner == null) PickupSpawner = FindObjectOfType<PickupSpawner>();
            if (PickupSpawner != null)
            {
                PickupSpawner.ClearAll();
                PickupSpawner.Initialize(Player.transform);
            }
            ApplyBikeAndUpgrades();
            return true;
        }

        private void CacheCameraEffects()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.farClipPlane = Mathf.Max(cam.farClipPlane, 900f);
                _cameraEffects = cam.GetComponent<CameraEffects>();
                if (_cameraEffects == null)
                {
                    _cameraEffects = cam.gameObject.AddComponent<CameraEffects>();
                }
            }
        }

        private IEnumerator CrashSequence()
        {
            _cameraEffects?.PlayCrashShake();
            Time.timeScale = 0.4f;
            yield return new WaitForSecondsRealtime(0.6f);
            Time.timeScale = 0f;
            UIController?.ShowGameOver(true, _distanceTraveled, _overtakes, _runCoins);
            _crashRoutine = null;
        }

        public void AddRunCoins(int amount)
        {
            if (amount <= 0) return;
            _runCoins += amount;
            if (mode == GameMode.Missions && MissionSystem != null && Save != null)
            {
                _runCoins += MissionSystem.AddProgress("coins", amount, true, Save.selectedMissionId);
                CheckMissionCompletion();
            }
        }

        private void CommitRunCoins()
        {
            if (Save == null) return;
            if (_runCoins <= 0) return;
            Save.coins += _runCoins;
            SaveSystem.Save(Save);
        }

        private void CheckMissionCompletion()
        {
            if (_missionCompletedThisRun || Save == null || MissionSystem == null) return;
            if (string.IsNullOrEmpty(Save.selectedMissionId)) return;

            MissionProgress progress = Save.missionProgress.Find(m => m.id == Save.selectedMissionId);
            if (progress != null && progress.completed)
            {
                _missionCompletedThisRun = true;
                CompleteMission();
            }
        }

        private void CompleteMission()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            CommitRunCoins();

            if (Player != null) Player.enabled = false;
            if (RoadSpawner != null) RoadSpawner.enabled = false;
            if (VehicleSpawner != null) VehicleSpawner.enabled = false;
            if (PickupSpawner != null) PickupSpawner.enabled = false;

            Time.timeScale = 0f;
            UIController?.ShowMissionComplete(true, _distanceTraveled, _overtakes, _runCoins);
        }

        private void ResetCoinMissionsForRun()
        {
            if (Save == null || Config == null) return;
            foreach (var mission in Config.missions)
            {
                if (mission.type != "coins") continue;
                MissionProgress progress = Save.missionProgress.Find(m => m.id == mission.id);
                if (progress != null && !progress.completed)
                {
                    progress.value = 0;
                }
            }
            SaveSystem.Save(Save);
        }

        private void ApplyTrafficMode()
        {
            if (Save == null) return;
            if (VehicleSpawner != null)
            {
                VehicleSpawner.allowOncoming = Save.twoWayTraffic;
            }
            if (RoadSpawner != null)
            {
                RoadSpawner.SetTrafficDirection(Save.twoWayTraffic);
            }
        }

        public void Pause(bool value)
        {
            _paused = value;
        }

        private void ApplyOrientation()
        {
            if (Save == null) return;
            if (Save.orientation == "landscape")
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
            else if (Save.orientation == "portrait")
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }
            else
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
            }
        }

        private void ApplyBackground()
        {
            if (BackgroundScroller == null || Save == null) return;
            BackgroundScroller.SetTheme(Save.background);
        }

        private void ApplyQualityFromSave()
        {
            string quality = Save != null ? Save.quality : "auto";
            if (string.IsNullOrEmpty(quality))
            {
                quality = "auto";
            }

            quality = quality.ToLowerInvariant();
            if (quality == "low")
            {
                ApplyQualityPreset(0, 0, 1, 0);
                return;
            }
            if (quality == "mid")
            {
                ApplyQualityPreset(0, 0, 0, 1);
                return;
            }
            if (quality == "high")
            {
                ApplyQualityPreset(2, 2, 0, 2);
                return;
            }

            // auto
            if (Application.isMobilePlatform)
            {
                ApplyQualityPreset(0, 0, 1, 1);
            }
            else
            {
                ApplyQualityPreset(2, 2, 0, 2);
            }
        }

        private static void ApplyQualityPreset(int shadowCascades, int antiAliasing, int mipLimit, int pixelLights)
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowCascades = shadowCascades;
            QualitySettings.shadows = shadowCascades > 0 ? ShadowQuality.All : ShadowQuality.Disable;
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.globalTextureMipmapLimit = mipLimit;
            QualitySettings.pixelLightCount = pixelLights;
            QualitySettings.anisotropicFiltering = antiAliasing > 0
                ? AnisotropicFiltering.Enable
                : AnisotropicFiltering.Disable;
        }

        private static void ApplyRuntimeQuality()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            QualitySettings.vSyncCount = 0;

            if (Application.isMobilePlatform)
            {
                QualitySettings.shadowCascades = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
                QualitySettings.globalTextureMipmapLimit = 1; // half res
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.realtimeReflectionProbes = false;
                // budgetedGPU is not available in older Unity versions
            }
        }
    }
}
