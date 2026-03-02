using TrafficRider.Gameplay;
using TrafficRider.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TrafficRider.Core
{
    public class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (IsMenuScene())
            {
                BootstrapMenu();
                return;
            }
            if (Object.FindObjectOfType<PlayerBikeController>() != null &&
                Object.FindObjectOfType<RoadSpawner>() != null &&
                Object.FindObjectOfType<VehicleSpawner>() != null)
            {
                return;
            }

            GameObject root = new GameObject("TrafficRiderRoot");

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
            PlayerBikeController player = playerObj.AddComponent<PlayerBikeController>();
            BikeVisual.BuildForBike(visual.transform, "bike_basic");

            GameObject cameraObj = new GameObject("MainCamera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 300f;
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 4.5f, -8f);
            cameraObj.transform.SetParent(playerObj.transform);
            cameraObj.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            cameraObj.AddComponent<TrafficRider.Gameplay.CameraEffects>();

            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject roadObj = new GameObject("RoadSpawner");
            RoadSpawner roadSpawner = roadObj.AddComponent<RoadSpawner>();

            GameObject trafficObj = new GameObject("VehicleSpawner");
            VehicleSpawner vehicleSpawner = trafficObj.AddComponent<VehicleSpawner>();

            GameObject pickupObj = new GameObject("PickupSpawner");
            PickupSpawner pickupSpawner = pickupObj.AddComponent<PickupSpawner>();

            GameObject backgroundObj = new GameObject("BackgroundScroller");
            BackgroundScroller background = backgroundObj.AddComponent<BackgroundScroller>();

            GameObject uiObj = new GameObject("UIController");
            UIController ui = uiObj.AddComponent<UIController>();

            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gm = gmObj.AddComponent<GameManager>();
            }
            gm.Initialize(player, roadSpawner, vehicleSpawner, ui);

            roadSpawner.Initialize(player.transform);
            vehicleSpawner.Initialize(player.transform);
            pickupSpawner.Initialize(player.transform);
            background.Initialize(player.transform);

            root.transform.position = Vector3.zero;
        }

        private static bool IsMenuScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.name.ToLowerInvariant().Contains("menu")) return true;
            return Object.FindObjectOfType<MenuSceneMarker>() != null;
        }

        private static void BootstrapMenu()
        {
            EnsureEventSystem();
            string sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();

            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gm = gmObj.AddComponent<GameManager>();
                gm.InitializeMenuOnly();
            }

            if (sceneName.Contains("bikemenu"))
            {
                BootstrapBikeMenuScene();
                return;
            }
            if (sceneName.Contains("missionmenu"))
            {
                BootstrapMissionMenuScene();
                return;
            }
            BootstrapMainMenuScene();
        }

        private static void BootstrapMainMenuScene()
        {
            if (Object.FindObjectOfType<StartMenuUIController>() != null) return;
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("MainCamera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 300f;
                cameraObj.tag = "MainCamera";
                cameraObj.transform.position = new Vector3(0f, 6.5f, -14f);
                cameraObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            }

            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightObj = new GameObject("DirectionalLight");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.0f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            GameObject backdropObj = new GameObject("MenuBackdrop");
            MenuBackdrop backdrop = backdropObj.AddComponent<MenuBackdrop>();
            backdrop.Build();

            GameObject uiObj = new GameObject("MenuUI");
            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();

            SettingsUIController settings = uiObj.AddComponent<SettingsUIController>();
            settings.Build(canvas.transform);
            settings.SetShowQualitySection(true);

            StartMenuUIController startMenu = uiObj.AddComponent<StartMenuUIController>();
            startMenu.Build(canvas.transform, settings);
            startMenu.Show();
            settings.SetHomeAction(() =>
            {
                settings.Hide();
                startMenu.Show();
                startMenu.SetInteractable(true);
                startMenu.SetDimmed(false);
            });
        }

        private static void BootstrapBikeMenuScene()
        {
            if (Object.FindObjectOfType<BikeMenuUIController>() != null)
            {
                BikeMenuUIController existing = Object.FindObjectOfType<BikeMenuUIController>();
                existing.SetHomeAction(() => SceneManager.LoadScene("MainMenu"));
                existing.Show();
                return;
            }

            EnsureMenuBackdrop();
            Canvas canvas = EnsureMenuCanvas();
            BikeMenuUIController bikeMenu = canvas.gameObject.AddComponent<BikeMenuUIController>();
            bikeMenu.Build(canvas.transform);
            bikeMenu.SetHomeAction(() => SceneManager.LoadScene("MainMenu"));
            bikeMenu.Show();
        }

        private static void BootstrapMissionMenuScene()
        {
            if (Object.FindObjectOfType<MissionMenuUIController>() != null)
            {
                MissionMenuUIController existing = Object.FindObjectOfType<MissionMenuUIController>();
                existing.SetHomeAction(() => SceneManager.LoadScene("MainMenu"));
                existing.Show();
                return;
            }

            EnsureMenuBackdrop();
            Canvas canvas = EnsureMenuCanvas();
            MissionMenuUIController missionMenu = canvas.gameObject.AddComponent<MissionMenuUIController>();
            missionMenu.Build(canvas.transform);
            missionMenu.SetBackAction(() => SceneManager.LoadScene("MainMenu"));
            missionMenu.Show();
        }

        private static Canvas EnsureMenuCanvas()
        {
            Canvas existing = Object.FindObjectOfType<Canvas>();
            if (existing != null)
            {
                if (existing.GetComponent<CanvasScaler>() == null) existing.gameObject.AddComponent<CanvasScaler>();
                if (existing.GetComponent<GraphicRaycaster>() == null) existing.gameObject.AddComponent<GraphicRaycaster>();
                return existing;
            }

            GameObject uiObj = new GameObject("MenuUI");
            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void EnsureMenuBackdrop()
        {
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("MainCamera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 300f;
                cameraObj.tag = "MainCamera";
                cameraObj.transform.position = new Vector3(0f, 6.5f, -14f);
                cameraObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            }

            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightObj = new GameObject("DirectionalLight");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.0f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            if (Object.FindObjectOfType<MenuBackdrop>() == null)
            {
                GameObject backdropObj = new GameObject("MenuBackdrop");
                MenuBackdrop backdrop = backdropObj.AddComponent<MenuBackdrop>();
                backdrop.Build();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
