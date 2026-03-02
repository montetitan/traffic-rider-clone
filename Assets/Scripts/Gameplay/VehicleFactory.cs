using UnityEngine;

namespace TrafficRider.Gameplay
{
    public static class VehicleFactory
    {
        public enum VehicleType
        {
            Car,
            Truck,
            Ambulance,
            Bus,
            Auto
        }

        public static GameObject Create(VehicleType type)
        {
            GameObject root = new GameObject(type.ToString());
            switch (type)
            {
                case VehicleType.Ambulance:
                    BuildAmbulance(root);
                    break;
                case VehicleType.Bus:
                    BuildBus(root);
                    break;
                case VehicleType.Truck:
                    BuildTruck(root);
                    break;
                case VehicleType.Auto:
                    BuildAuto(root);
                    break;
                default:
                    BuildCar(root);
                    break;
            }
            return root;
        }

        private static Material Mat(Color color, float metallic = 0.1f, float smooth = 0.4f)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smooth);
            return mat;
        }

        private static void BuildCar(GameObject root)
        {
            Material body = Mat(RandomBodyColor(), 0.2f, 0.6f);
            Material glass = Mat(new Color(0.2f, 0.3f, 0.35f), 0.5f, 0.9f);
            Material trim = Mat(new Color(0.1f, 0.1f, 0.1f), 0.0f, 0.2f);
            Material light = Mat(new Color(0.7f, 0.7f, 0.7f), 0.0f, 0.6f);
            Material tail = Mat(new Color(0.9f, 0.1f, 0.1f), 0.0f, 0.6f);

            GameObject bodyBlock = Cube(root, "Body", new Vector3(1.6f, 0.6f, 3.2f), new Vector3(0f, 0.6f, 0f), body);
            GameObject top = Cube(root, "Cab", new Vector3(1.1f, 0.45f, 1.4f), new Vector3(0f, 0.95f, 0f), body);
            Cube(root, "Glass", new Vector3(0.9f, 0.3f, 1.0f), new Vector3(0f, 1.02f, 0.1f), glass);
            Cube(root, "BumperFront", new Vector3(1.7f, 0.2f, 0.3f), new Vector3(0f, 0.35f, 1.65f), trim);
            Cube(root, "BumperRear", new Vector3(1.7f, 0.2f, 0.3f), new Vector3(0f, 0.35f, -1.65f), trim);
            Cube(root, "HeadlightL", new Vector3(0.2f, 0.15f, 0.05f), new Vector3(-0.55f, 0.55f, 1.65f), light);
            Cube(root, "HeadlightR", new Vector3(0.2f, 0.15f, 0.05f), new Vector3(0.55f, 0.55f, 1.65f), light);
            Cube(root, "TaillightL", new Vector3(0.2f, 0.12f, 0.05f), new Vector3(-0.55f, 0.55f, -1.65f), tail);
            Cube(root, "TaillightR", new Vector3(0.2f, 0.12f, 0.05f), new Vector3(0.55f, 0.55f, -1.65f), tail);

            bool taxi = body.color.g > 0.5f && body.color.r > 0.8f;
            bool police = !taxi && Random.value < 0.2f;

            // Taxi livery (checker + roof light) for yellow-ish cars
            if (taxi)
            {
                Material checker = Mat(new Color(0.05f, 0.05f, 0.05f), 0.0f, 0.2f);
                Cube(root, "TaxiCheckerL", new Vector3(0.05f, 0.12f, 1.6f), new Vector3(-0.85f, 0.6f, 0f), checker);
                Cube(root, "TaxiCheckerR", new Vector3(0.05f, 0.12f, 1.6f), new Vector3(0.85f, 0.6f, 0f), checker);
                Cube(root, "TaxiLight", new Vector3(0.4f, 0.15f, 0.25f), new Vector3(0f, 1.25f, 0.0f), Mat(new Color(0.6f, 0.6f, 0.4f)));
            }

            // Police livery (stripe + light bar)
            if (police)
            {
                Material stripe = Mat(new Color(0.05f, 0.2f, 0.6f), 0.0f, 0.3f);
                Cube(root, "PoliceStripeL", new Vector3(0.05f, 0.18f, 2.2f), new Vector3(-0.85f, 0.6f, 0f), stripe);
                Cube(root, "PoliceStripeR", new Vector3(0.05f, 0.18f, 2.2f), new Vector3(0.85f, 0.6f, 0f), stripe);
                Cube(root, "PoliceLightBar", new Vector3(0.6f, 0.15f, 0.3f), new Vector3(0f, 1.25f, 0.0f), Mat(new Color(0.2f, 0.3f, 0.9f)));
            }
            AddWobbleIndicator(root);
        }

        private static void BuildAmbulance(GameObject root)
        {
            Material body = Mat(new Color(0.75f, 0.75f, 0.75f), 0.1f, 0.5f);
            Material glass = Mat(new Color(0.2f, 0.3f, 0.35f), 0.5f, 0.9f);
            Material trim = Mat(new Color(0.15f, 0.15f, 0.15f), 0.0f, 0.2f);
            Material light = Mat(new Color(0.7f, 0.2f, 0.2f), 0.0f, 0.6f);
            Material head = Mat(new Color(0.8f, 0.8f, 0.7f), 0.0f, 0.6f);
            Material tail = Mat(new Color(0.9f, 0.1f, 0.1f), 0.0f, 0.6f);

            Cube(root, "Body", new Vector3(1.8f, 1.2f, 3.8f), new Vector3(0f, 0.9f, 0f), body);
            Cube(root, "Cab", new Vector3(1.6f, 0.9f, 1.6f), new Vector3(0f, 1.15f, 0.7f), body);
            Cube(root, "Glass", new Vector3(1.2f, 0.5f, 0.9f), new Vector3(0f, 1.25f, 0.9f), glass);
            Cube(root, "Stripe", new Vector3(1.7f, 0.2f, 3.7f), new Vector3(0f, 0.75f, 0f), Mat(new Color(0.9f, 0.1f, 0.1f)));
            Cube(root, "LightBar", new Vector3(0.9f, 0.15f, 0.4f), new Vector3(0f, 1.6f, 0f), light);
            Cube(root, "Bumper", new Vector3(1.9f, 0.2f, 0.3f), new Vector3(0f, 0.35f, 1.95f), trim);
            Cube(root, "HeadlightL", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(-0.6f, 0.55f, 1.95f), head);
            Cube(root, "HeadlightR", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(0.6f, 0.55f, 1.95f), head);
            Cube(root, "TaillightL", new Vector3(0.22f, 0.12f, 0.05f), new Vector3(-0.6f, 0.6f, -1.95f), tail);
            Cube(root, "TaillightR", new Vector3(0.22f, 0.12f, 0.05f), new Vector3(0.6f, 0.6f, -1.95f), tail);

            // Red cross decals
            Material cross = Mat(new Color(0.85f, 0.1f, 0.1f));
            Cube(root, "CrossH", new Vector3(0.5f, 0.12f, 0.1f), new Vector3(0f, 1.0f, 0.2f), cross);
            Cube(root, "CrossV", new Vector3(0.12f, 0.5f, 0.1f), new Vector3(0f, 1.0f, 0.2f), cross);
            AddWobbleIndicator(root);
        }

        private static void BuildBus(GameObject root)
        {
            Material body = Mat(new Color(0.45f, 0.45f, 0.5f), 0.05f, 0.4f);
            Material glass = Mat(new Color(0.2f, 0.3f, 0.35f), 0.5f, 0.9f);
            Material trim = Mat(new Color(0.1f, 0.1f, 0.1f), 0.0f, 0.2f);
            Material head = Mat(new Color(0.8f, 0.8f, 0.7f), 0.0f, 0.6f);
            Material tail = Mat(new Color(0.9f, 0.1f, 0.1f), 0.0f, 0.6f);

            Cube(root, "Body", new Vector3(2.3f, 1.6f, 5.4f), new Vector3(0f, 1.15f, 0f), body);
            for (int i = -2; i <= 2; i++)
            {
                Cube(root, "Window" + i, new Vector3(1.8f, 0.45f, 0.4f), new Vector3(0f, 1.5f, i * 1.0f), glass);
            }
            Cube(root, "Bumper", new Vector3(2.4f, 0.25f, 0.35f), new Vector3(0f, 0.4f, 2.75f), trim);
            Cube(root, "HeadlightL", new Vector3(0.3f, 0.18f, 0.05f), new Vector3(-0.8f, 0.55f, 2.75f), head);
            Cube(root, "HeadlightR", new Vector3(0.3f, 0.18f, 0.05f), new Vector3(0.8f, 0.55f, 2.75f), head);
            Cube(root, "TaillightL", new Vector3(0.3f, 0.15f, 0.05f), new Vector3(-0.8f, 0.55f, -2.75f), tail);
            Cube(root, "TaillightR", new Vector3(0.3f, 0.15f, 0.05f), new Vector3(0.8f, 0.55f, -2.75f), tail);
            Cube(root, "Stripe", new Vector3(2.2f, 0.2f, 5.2f), new Vector3(0f, 0.75f, 0f), Mat(new Color(0.2f, 0.4f, 0.8f)));
            Cube(root, "RouteNumber", new Vector3(0.8f, 0.35f, 0.05f), new Vector3(0f, 1.35f, 2.6f), Mat(new Color(0.05f, 0.05f, 0.05f)));
            AddWobbleIndicator(root);
        }

        private static void BuildTruck(GameObject root)
        {
            Material cab = Mat(new Color(0.2f, 0.45f, 0.85f), 0.2f, 0.6f);
            Material trailer = Mat(new Color(0.75f, 0.75f, 0.75f), 0.05f, 0.3f);
            Material glass = Mat(new Color(0.2f, 0.3f, 0.35f), 0.5f, 0.9f);
            Material head = Mat(new Color(0.8f, 0.8f, 0.7f), 0.0f, 0.6f);
            Material tail = Mat(new Color(0.9f, 0.1f, 0.1f), 0.0f, 0.6f);

            Cube(root, "Cab", new Vector3(1.6f, 1.0f, 1.6f), new Vector3(0f, 0.9f, 1.3f), cab);
            Cube(root, "CabGlass", new Vector3(1.0f, 0.5f, 0.6f), new Vector3(0f, 1.05f, 1.4f), glass);
            Cube(root, "Trailer", new Vector3(2.2f, 1.3f, 4.2f), new Vector3(0f, 1.0f, -1.3f), trailer);
            Cube(root, "LogoStripe", new Vector3(2.1f, 0.2f, 3.8f), new Vector3(0f, 1.1f, -1.3f), Mat(new Color(0.1f, 0.6f, 0.2f)));
            Cube(root, "HeadlightL", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(-0.55f, 0.6f, 2.1f), head);
            Cube(root, "HeadlightR", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(0.55f, 0.6f, 2.1f), head);
            Cube(root, "TaillightL", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(-0.9f, 0.6f, -3.3f), tail);
            Cube(root, "TaillightR", new Vector3(0.25f, 0.15f, 0.05f), new Vector3(0.9f, 0.6f, -3.3f), tail);
            AddWobbleIndicator(root);
        }

        private static void BuildAuto(GameObject root)
        {
            Material body = Mat(new Color(0.2f, 0.6f, 0.45f), 0.1f, 0.5f);
            Material trim = Mat(new Color(0.1f, 0.1f, 0.1f), 0.0f, 0.2f);
            Material head = Mat(new Color(0.8f, 0.8f, 0.7f), 0.0f, 0.6f);
            Material tail = Mat(new Color(0.9f, 0.1f, 0.1f), 0.0f, 0.6f);

            Cube(root, "Body", new Vector3(1.2f, 0.9f, 2.4f), new Vector3(0f, 0.75f, 0f), body);
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localScale = new Vector3(0.7f, 0.2f, 0.7f);
            roof.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            roof.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            roof.GetComponent<Renderer>().material = trim;

            // Police-style livery for autos: small light bar
            Cube(root, "RoofLight", new Vector3(0.5f, 0.15f, 0.25f), new Vector3(0f, 1.4f, 0.0f), Mat(new Color(0.2f, 0.3f, 0.9f)));
            Cube(root, "HeadlightL", new Vector3(0.18f, 0.12f, 0.05f), new Vector3(-0.4f, 0.5f, 1.25f), head);
            Cube(root, "HeadlightR", new Vector3(0.18f, 0.12f, 0.05f), new Vector3(0.4f, 0.5f, 1.25f), head);
            Cube(root, "TaillightL", new Vector3(0.18f, 0.12f, 0.05f), new Vector3(-0.4f, 0.5f, -1.25f), tail);
            Cube(root, "TaillightR", new Vector3(0.18f, 0.12f, 0.05f), new Vector3(0.4f, 0.5f, -1.25f), tail);
            AddWobbleIndicator(root);
        }

        private static GameObject Cube(GameObject root, string name, Vector3 scale, Vector3 pos, Material mat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(root.transform);
            cube.transform.localScale = scale;
            cube.transform.localPosition = pos;
            cube.GetComponent<Renderer>().material = mat;
            return cube;
        }

        private static void AddWobbleIndicator(GameObject root)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.name = "WobbleIndicator";
            indicator.transform.SetParent(root.transform);
            indicator.transform.localScale = new Vector3(0.2f, 0.05f, 0.6f);
            indicator.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            indicator.GetComponent<Renderer>().material = Mat(new Color(0.9f, 0.85f, 0.2f), 0.0f, 0.2f);
        }

        private static Color RandomBodyColor()
        {
            Color[] colors =
            {
                new Color(0.18f, 0.45f, 0.7f),
                new Color(0.65f, 0.2f, 0.2f),
                new Color(0.2f, 0.55f, 0.3f),
                new Color(0.55f, 0.45f, 0.15f),
                new Color(0.6f, 0.6f, 0.6f)
            };
            return colors[Random.Range(0, colors.Length)];
        }
    }
}
