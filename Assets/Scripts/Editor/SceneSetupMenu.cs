#if UNITY_EDITOR
using TrafficRider.Core;
using TrafficRider.Gameplay;
using TrafficRider.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TrafficRider.Editor
{
    public static class SceneSetupMenu
    {
        [MenuItem("TrafficRider/Create Main Menu Scene")]
        public static void CreateMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject marker = new GameObject("MenuSceneMarker");
            marker.AddComponent<TrafficRider.UI.MenuSceneMarker>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewGame.unity", true)
            };

            Debug.Log("Menu scene created at Assets/Scenes/MainMenu.unity");
        }

        [MenuItem("TrafficRider/Create Bike Selection Menu Scene")]
        public static void CreateBikeSelectionMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject marker = new GameObject("MenuSceneMarker");
            marker.AddComponent<TrafficRider.UI.MenuSceneMarker>();

            GameObject cameraObj = new GameObject("MainCamera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 300f;
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 6.5f, -14f);
            cameraObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject backdropObj = new GameObject("MenuBackdrop");
            MenuBackdrop backdrop = backdropObj.AddComponent<MenuBackdrop>();
            backdrop.Build();

            GameObject uiObj = new GameObject("BikeMenuUI");
            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();

            BikeMenuUIController bikeMenu = uiObj.AddComponent<BikeMenuUIController>();
            bikeMenu.Build(canvas.transform);
            bikeMenu.Show();

            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.InitializeMenuOnly();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/BikeMenu.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/BikeMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewGame.unity", true)
            };

            Debug.Log("Bike selection menu scene created at Assets/Scenes/BikeMenu.unity");
        }

        [MenuItem("TrafficRider/Create Mission Selection Menu Scene")]
        public static void CreateMissionSelectionMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject marker = new GameObject("MenuSceneMarker");
            marker.AddComponent<TrafficRider.UI.MenuSceneMarker>();

            GameObject cameraObj = new GameObject("MainCamera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 300f;
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 6.5f, -14f);
            cameraObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject backdropObj = new GameObject("MenuBackdrop");
            MenuBackdrop backdrop = backdropObj.AddComponent<MenuBackdrop>();
            backdrop.Build();

            GameObject uiObj = new GameObject("MissionMenuUI");
            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();

            MissionMenuUIController missionMenu = uiObj.AddComponent<MissionMenuUIController>();
            missionMenu.Build(canvas.transform);
            missionMenu.Show();

            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.InitializeMenuOnly();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MissionMenu.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/BikeMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MissionMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewGame.unity", true)
            };

            Debug.Log("Mission selection menu scene created at Assets/Scenes/MissionMenu.unity");
        }

        [MenuItem("TrafficRider/Create New Game Scene")]
        public static void CreateMainScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject playerObj = new GameObject("PlayerBike");
            playerObj.transform.position = new Vector3(0f, 1.0f, 0f);

            CapsuleCollider collider = playerObj.AddComponent<CapsuleCollider>();
            collider.radius = 0.35f;
            collider.height = 1.6f;
            collider.center = new Vector3(0f, 1.1f, 0f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(playerObj.transform);
            visual.transform.localPosition = Vector3.zero;

            Rigidbody rb = playerObj.AddComponent<Rigidbody>();
            rb.mass = 120f;
            rb.drag = 0.5f;
            playerObj.AddComponent<PlayerBikeController>();
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
            roadObj.AddComponent<RoadSpawner>();

            GameObject trafficObj = new GameObject("VehicleSpawner");
            trafficObj.AddComponent<VehicleSpawner>();

            GameObject pickupObj = new GameObject("PickupSpawner");
            pickupObj.AddComponent<PickupSpawner>();

            GameObject backgroundObj = new GameObject("BackgroundScroller");
            backgroundObj.AddComponent<BackgroundScroller>();

            GameObject uiObj = new GameObject("UIController");
            uiObj.AddComponent<UIController>();

            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/NewGame.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/NewGame.unity", true) };

            Debug.Log("Main scene created at Assets/Scenes/NewGame.unity");
        }
    }
}
#endif
