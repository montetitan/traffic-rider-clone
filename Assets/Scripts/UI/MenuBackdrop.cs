using TrafficRider.Gameplay;
using TrafficRider.Core;
using TrafficRider.Data;
using UnityEngine;

namespace TrafficRider.UI
{
    public class MenuBackdrop : MonoBehaviour
    {
        private GameObject _root;
        private GameObject _bikeRoot;
        private GameObject _leftGround;
        private GameObject _rightGround;
        private string _currentTheme;
        private string _currentBikeId;

        private void Awake()
        {
            Build();
        }

        public void Build()
        {
            if (_root != null) return;
            _root = new GameObject("MenuBackdropRoot");
            _root.transform.position = Vector3.zero;

            // Road slab
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "MenuRoad";
            road.transform.SetParent(_root.transform);
            road.transform.position = new Vector3(0f, -0.5f, 8f);
            road.transform.localScale = new Vector3(20f, 0.4f, 30f);
            road.GetComponent<Renderer>().material = BuildMat(new Color(0.2f, 0.2f, 0.2f));

            // Center dashed line
            for (int i = 0; i < 10; i++)
            {
                GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = "MenuDash";
                dash.transform.SetParent(_root.transform);
                dash.transform.position = new Vector3(0f, -0.2f, 2f + i * 2.5f);
                dash.transform.localScale = new Vector3(0.1f, 0.05f, 1f);
                dash.GetComponent<Renderer>().material = BuildMat(new Color(0.65f, 0.3f, 0.05f));
            }

            // Guard rails
            CreateRail(new Vector3(-10f, 0.0f, 8f), new Color(0.55f, 0.12f, 0.12f));
            CreateRail(new Vector3(10f, 0.0f, 8f), new Color(0.55f, 0.5f, 0.1f));

            // Ground strips (theme colored)
            _leftGround = CreateGround(new Vector3(-15f, -0.3f, 8f));
            _rightGround = CreateGround(new Vector3(15f, -0.3f, 8f));

            // Bike preview
            _bikeRoot = new GameObject("MenuBike");
            _bikeRoot.transform.SetParent(_root.transform);
            _bikeRoot.transform.position = new Vector3(0f, 0.2f, 4f);
            BikeVisual.BuildForBike(_bikeRoot.transform, "bike_basic");

            // Static traffic props
            AddVehicleProp(new Vector3(-3f, 0f, 12f), 0f);
            AddVehicleProp(new Vector3(3f, 0f, 16f), 180f);

            // City blocks
            AddBuildings(-16f);
            AddBuildings(16f);

            ApplyFromSave();
        }

        private void Update()
        {
            if (_root == null)
            {
                Build();
                if (_root == null) return;
            }

            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Save == null) return;

            if (_currentTheme != gm.Save.background)
            {
                SetTheme(gm.Save.background);
            }
            if (_currentBikeId != gm.Save.selectedBikeId)
            {
                SetBike(gm.Save.selectedBikeId, gm.Config);
            }
        }

        private void CreateRail(Vector3 pos, Color color)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "MenuRail";
            rail.transform.SetParent(_root.transform);
            rail.transform.position = pos;
            rail.transform.localScale = new Vector3(0.2f, 0.2f, 30f);
            rail.GetComponent<Renderer>().material = BuildMat(color);
        }

        private void AddVehicleProp(Vector3 pos, float yRot)
        {
            GameObject vehicle = VehicleFactory.Create(VehicleFactory.VehicleType.Car);
            vehicle.transform.SetParent(_root.transform);
            vehicle.transform.position = pos;
            vehicle.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        }

        private void AddBuildings(float x)
        {
            Color[] palette = GetBuildingPalette();
            for (int i = 0; i < 6; i++)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = "Building";
                block.transform.SetParent(_root.transform);
                block.transform.position = new Vector3(x, 2f + i, 4f + i * 4f);
                block.transform.localScale = new Vector3(4f, 4f + i * 0.7f, 4f);
                Color baseColor = palette[Mathf.Clamp(i % palette.Length, 0, palette.Length - 1)];
                block.GetComponent<Renderer>().material = BuildMat(baseColor);
            }
        }

        private GameObject CreateGround(Vector3 pos)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "MenuGround";
            ground.transform.SetParent(_root.transform);
            ground.transform.position = pos;
            ground.transform.localScale = new Vector3(12f, 0.4f, 30f);
            ground.GetComponent<Renderer>().material = BuildMat(GetGroundColor());
            return ground;
        }

        private void ApplyFromSave()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Save == null) return;
            SetTheme(gm.Save.background);
            SetBike(gm.Save.selectedBikeId, gm.Config);
        }

        public void SetTheme(string theme)
        {
            if (_root == null)
            {
                Build();
                if (_root == null) return;
            }

            if (string.IsNullOrEmpty(theme)) return;
            string t = theme.ToLowerInvariant();
            if (_currentTheme == t) return;
            _currentTheme = t;

            if (_leftGround != null)
            {
                _leftGround.GetComponent<Renderer>().material = BuildMat(GetGroundColor());
            }
            if (_rightGround != null)
            {
                _rightGround.GetComponent<Renderer>().material = BuildMat(GetGroundColor());
            }

            // Rebuild buildings colors
            foreach (Transform child in _root.transform)
            {
                if (child.name.StartsWith("Building"))
                {
                    Destroy(child.gameObject);
                }
            }
            AddBuildings(-16f);
            AddBuildings(16f);
        }

        public void SetBike(string bikeId, GameConfig config)
        {
            if (_root == null)
            {
                Build();
                if (_root == null) return;
            }

            if (string.IsNullOrEmpty(bikeId)) return;
            if (_currentBikeId == bikeId) return;
            _currentBikeId = bikeId;
            if (_bikeRoot != null)
            {
                Destroy(_bikeRoot);
            }

            _bikeRoot = new GameObject("MenuBike");
            _bikeRoot.transform.SetParent(_root.transform);
            _bikeRoot.transform.position = new Vector3(0f, 0.2f, 4f);

            Color frameColor = GetBikeColor(bikeId, config);
            BikeVisual.BuildForBike(_bikeRoot.transform, bikeId, frameColor);
        }

        private Color GetBikeColor(string bikeId, GameConfig config)
        {
            if (config != null)
            {
                BikeConfig cfg = config.bikes.Find(b => b.id == bikeId);
                if (cfg != null)
                {
                    float t = Mathf.InverseLerp(0f, 120f, cfg.maxSpeed);
                    return Color.Lerp(new Color(0.15f, 0.6f, 0.9f), new Color(0.9f, 0.25f, 0.2f), t);
                }
            }

            int hash = bikeId.GetHashCode();
            Random.InitState(hash);
            return new Color(0.3f + Random.value * 0.6f, 0.3f + Random.value * 0.6f, 0.3f + Random.value * 0.6f);
        }

        private Color[] GetBuildingPalette()
        {
            switch (_currentTheme)
            {
                case "desert":
                    return new[] { new Color(0.55f, 0.45f, 0.3f), new Color(0.6f, 0.5f, 0.35f), new Color(0.5f, 0.4f, 0.25f) };
                case "greenery":
                    return new[] { new Color(0.2f, 0.35f, 0.22f), new Color(0.25f, 0.4f, 0.3f), new Color(0.3f, 0.45f, 0.28f) };
                case "wasteland":
                    return new[] { new Color(0.25f, 0.25f, 0.25f), new Color(0.3f, 0.2f, 0.2f), new Color(0.2f, 0.2f, 0.22f) };
                default:
                    return new[] { new Color(0.25f, 0.3f, 0.35f), new Color(0.3f, 0.25f, 0.22f), new Color(0.28f, 0.24f, 0.32f) };
            }
        }

        private Color GetGroundColor()
        {
            switch (_currentTheme)
            {
                case "desert":
                    return new Color(0.35f, 0.28f, 0.18f);
                case "greenery":
                    return new Color(0.12f, 0.2f, 0.12f);
                case "wasteland":
                    return new Color(0.18f, 0.18f, 0.18f);
                default:
                    return new Color(0.12f, 0.12f, 0.12f);
            }
        }

        private static Material BuildMat(Color color)
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0f);
            }
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }
            return mat;
        }
    }
}
