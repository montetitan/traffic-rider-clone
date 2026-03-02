using UnityEngine;

namespace TrafficRider.Gameplay
{
    public static class BikeVisual
    {
        public static void BuildForBike(Transform root, string bikeId)
        {
            BuildForBike(root, bikeId, PickBikeColor(bikeId));
        }

        public static void BuildForBike(Transform root, string bikeId, Color frameColor)
        {
            if (root == null) return;
            ClearChildren(root);
            BuildStandard(root, frameColor);
            ApplyVariantKit(root, bikeId);
        }

        public static void BuildStandard(Transform root)
        {
            BuildStandard(root, new Color(0.15f, 0.6f, 0.9f));
        }

        public static void BuildStandard(Transform root, Color frameColor)
        {
            Material frameMat = new Material(Shader.Find("Standard"))
            {
                color = frameColor
            };
            Material darkMat = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.1f, 0.1f, 0.1f)
            };
            Material metalMat = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.6f, 0.6f, 0.6f)
            };
            Material lightMat = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.9f, 0.9f, 0.7f)
            };

            GameObject wheelFront = BuildWheel("FrontWheel", darkMat);
            wheelFront.transform.SetParent(root);
            wheelFront.transform.localPosition = new Vector3(0f, 0.45f, 1.0f);

            GameObject wheelRear = BuildWheel("RearWheel", darkMat);
            wheelRear.transform.SetParent(root);
            wheelRear.transform.localPosition = new Vector3(0f, 0.45f, -1.0f);

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(root);
            frame.transform.localScale = new Vector3(0.25f, 0.25f, 1.4f);
            frame.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            frame.GetComponent<Renderer>().material = frameMat;

            GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tank.name = "Tank";
            tank.transform.SetParent(root);
            tank.transform.localScale = new Vector3(0.35f, 0.25f, 0.45f);
            tank.transform.localPosition = new Vector3(0f, 0.95f, 0.25f);
            tank.GetComponent<Renderer>().material = frameMat;

            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(root);
            seat.transform.localScale = new Vector3(0.3f, 0.12f, 0.5f);
            seat.transform.localPosition = new Vector3(0f, 1.0f, -0.1f);
            seat.GetComponent<Renderer>().material = darkMat;

            GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            engine.name = "Engine";
            engine.transform.SetParent(root);
            engine.transform.localScale = new Vector3(0.35f, 0.3f, 0.45f);
            engine.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            engine.GetComponent<Renderer>().material = metalMat;

            GameObject exhaust = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            exhaust.name = "Exhaust";
            exhaust.transform.SetParent(root);
            exhaust.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
            exhaust.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            exhaust.transform.localPosition = new Vector3(-0.25f, 0.6f, -0.6f);
            exhaust.GetComponent<Renderer>().material = metalMat;

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(root);
            handle.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            handle.transform.localPosition = new Vector3(0f, 1.15f, 0.7f);
            handle.GetComponent<Renderer>().material = darkMat;

            GameObject fork = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fork.name = "Fork";
            fork.transform.SetParent(root);
            fork.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            fork.transform.localPosition = new Vector3(0f, 0.8f, 0.75f);
            fork.GetComponent<Renderer>().material = darkMat;

            GameObject headlight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headlight.name = "Headlight";
            headlight.transform.SetParent(root);
            headlight.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            headlight.transform.localPosition = new Vector3(0f, 1.0f, 1.1f);
            headlight.GetComponent<Renderer>().material = lightMat;

            GameObject fenderFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fenderFront.name = "FrontFender";
            fenderFront.transform.SetParent(root);
            fenderFront.transform.localScale = new Vector3(0.35f, 0.1f, 0.4f);
            fenderFront.transform.localPosition = new Vector3(0f, 0.7f, 1.0f);
            fenderFront.GetComponent<Renderer>().material = frameMat;

            GameObject fenderRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fenderRear.name = "RearFender";
            fenderRear.transform.SetParent(root);
            fenderRear.transform.localScale = new Vector3(0.35f, 0.1f, 0.4f);
            fenderRear.transform.localPosition = new Vector3(0f, 0.7f, -1.0f);
            fenderRear.GetComponent<Renderer>().material = frameMat;

            BuildDummyRider(root, darkMat, metalMat);
        }

        private static void BuildDummyRider(Transform root, Material suitMat, Material helmetMat)
        {
            GameObject rider = new GameObject("DummyRider");
            rider.transform.SetParent(root);
            rider.transform.localPosition = new Vector3(0f, 0.95f, -0.05f);
            rider.transform.localRotation = Quaternion.identity;

            Material skinMat = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.86f, 0.74f, 0.62f)
            };

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(rider.transform);
            torso.transform.localScale = new Vector3(0.22f, 0.22f, 0.18f);
            torso.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            torso.GetComponent<Renderer>().material = suitMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(rider.transform);
            head.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            head.transform.localPosition = new Vector3(0f, 0.55f, 0.08f);
            head.GetComponent<Renderer>().material = skinMat;

            GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "Helmet";
            helmet.transform.SetParent(rider.transform);
            helmet.transform.localScale = new Vector3(0.22f, 0.16f, 0.22f);
            helmet.transform.localPosition = new Vector3(0f, 0.58f, 0.08f);
            helmet.GetComponent<Renderer>().material = helmetMat;

            GameObject armLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armLeft.name = "ArmLeft";
            armLeft.transform.SetParent(rider.transform);
            armLeft.transform.localScale = new Vector3(0.08f, 0.08f, 0.28f);
            armLeft.transform.localPosition = new Vector3(-0.18f, 0.28f, 0.22f);
            armLeft.transform.localRotation = Quaternion.Euler(10f, 0f, 25f);
            armLeft.GetComponent<Renderer>().material = suitMat;

            GameObject armRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armRight.name = "ArmRight";
            armRight.transform.SetParent(rider.transform);
            armRight.transform.localScale = new Vector3(0.08f, 0.08f, 0.28f);
            armRight.transform.localPosition = new Vector3(0.18f, 0.28f, 0.22f);
            armRight.transform.localRotation = Quaternion.Euler(10f, 0f, -25f);
            armRight.GetComponent<Renderer>().material = suitMat;
        }

        private static GameObject BuildWheel(string name, Material mat)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheel.GetComponent<Renderer>().material = mat;
            return wheel;
        }

        private static void ApplyVariantKit(Transform root, string bikeId)
        {
            string id = string.IsNullOrEmpty(bikeId) ? "bike_basic" : bikeId.ToLowerInvariant();
            Material kitMat = new Material(Shader.Find("Standard")) { color = new Color(0.12f, 0.12f, 0.12f) };
            Material accent = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.8f, 0.8f) };

            if (id == "bike_basic") { AddTopBox(root, kitMat); return; }
            if (id == "bike_sport") { AddFrontFairing(root, kitMat); return; }
            if (id == "bike_super") { AddTailSpoiler(root, kitMat); return; }
            if (id == "bike_extra_1") { AddCrashBars(root, accent); return; }
            if (id == "bike_extra_2") { AddSideBags(root, kitMat); return; }
            if (id == "bike_extra_3") { AddTallWindshield(root, accent); return; }
            if (id == "bike_extra_4") { AddDualExhaust(root, accent); return; }
            if (id == "bike_extra_5") { AddWinglets(root, kitMat); return; }
            if (id == "bike_extra_6") { AddLowHandlebar(root, accent); return; }
            if (id == "bike_extra_7") { AddFrontFairing(root, kitMat); AddTailSpoiler(root, kitMat); return; }
            if (id == "bike_extra_8") { AddTopBox(root, kitMat); AddSideBags(root, kitMat); return; }
            if (id == "bike_extra_9") { AddTallWindshield(root, accent); AddCrashBars(root, accent); return; }
            if (id == "bike_extra_10") { AddWinglets(root, kitMat); AddDualExhaust(root, accent); return; }
        }

        private static void AddTopBox(Transform root, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "TopBox";
            box.transform.SetParent(root);
            box.transform.localScale = new Vector3(0.35f, 0.22f, 0.32f);
            box.transform.localPosition = new Vector3(0f, 1.15f, -0.75f);
            box.GetComponent<Renderer>().material = mat;
        }

        private static void AddFrontFairing(Transform root, Material mat)
        {
            GameObject fairing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fairing.name = "FrontFairing";
            fairing.transform.SetParent(root);
            fairing.transform.localScale = new Vector3(0.42f, 0.4f, 0.38f);
            fairing.transform.localPosition = new Vector3(0f, 1.0f, 0.9f);
            fairing.GetComponent<Renderer>().material = mat;
        }

        private static void AddTailSpoiler(Transform root, Material mat)
        {
            GameObject spoiler = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spoiler.name = "TailSpoiler";
            spoiler.transform.SetParent(root);
            spoiler.transform.localScale = new Vector3(0.55f, 0.05f, 0.2f);
            spoiler.transform.localPosition = new Vector3(0f, 1.2f, -0.65f);
            spoiler.GetComponent<Renderer>().material = mat;
        }

        private static void AddCrashBars(Transform root, Material mat)
        {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "CrashBarL";
            left.transform.SetParent(root);
            left.transform.localScale = new Vector3(0.05f, 0.35f, 0.5f);
            left.transform.localPosition = new Vector3(-0.28f, 0.72f, 0f);
            left.GetComponent<Renderer>().material = mat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "CrashBarR";
            right.transform.SetParent(root);
            right.transform.localScale = new Vector3(0.05f, 0.35f, 0.5f);
            right.transform.localPosition = new Vector3(0.28f, 0.72f, 0f);
            right.GetComponent<Renderer>().material = mat;
        }

        private static void AddSideBags(Transform root, Material mat)
        {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "SideBagL";
            left.transform.SetParent(root);
            left.transform.localScale = new Vector3(0.18f, 0.22f, 0.45f);
            left.transform.localPosition = new Vector3(-0.3f, 0.88f, -0.45f);
            left.GetComponent<Renderer>().material = mat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "SideBagR";
            right.transform.SetParent(root);
            right.transform.localScale = new Vector3(0.18f, 0.22f, 0.45f);
            right.transform.localPosition = new Vector3(0.3f, 0.88f, -0.45f);
            right.GetComponent<Renderer>().material = mat;
        }

        private static void AddTallWindshield(Transform root, Material mat)
        {
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "TallWindshield";
            glass.transform.SetParent(root);
            glass.transform.localScale = new Vector3(0.26f, 0.42f, 0.05f);
            glass.transform.localPosition = new Vector3(0f, 1.24f, 0.86f);
            glass.GetComponent<Renderer>().material = mat;
        }

        private static void AddDualExhaust(Transform root, Material mat)
        {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            left.name = "ExhaustDualL";
            left.transform.SetParent(root);
            left.transform.localScale = new Vector3(0.06f, 0.45f, 0.06f);
            left.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            left.transform.localPosition = new Vector3(-0.24f, 0.58f, -0.65f);
            left.GetComponent<Renderer>().material = mat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            right.name = "ExhaustDualR";
            right.transform.SetParent(root);
            right.transform.localScale = new Vector3(0.06f, 0.45f, 0.06f);
            right.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            right.transform.localPosition = new Vector3(0.24f, 0.58f, -0.65f);
            right.GetComponent<Renderer>().material = mat;
        }

        private static void AddWinglets(Transform root, Material mat)
        {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "WingletL";
            left.transform.SetParent(root);
            left.transform.localScale = new Vector3(0.18f, 0.03f, 0.2f);
            left.transform.localPosition = new Vector3(-0.26f, 1.02f, 0.82f);
            left.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            left.GetComponent<Renderer>().material = mat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "WingletR";
            right.transform.SetParent(root);
            right.transform.localScale = new Vector3(0.18f, 0.03f, 0.2f);
            right.transform.localPosition = new Vector3(0.26f, 1.02f, 0.82f);
            right.transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
            right.GetComponent<Renderer>().material = mat;
        }

        private static void AddLowHandlebar(Transform root, Material mat)
        {
            Transform handle = root.Find("Handle");
            if (handle != null)
            {
                handle.localPosition = new Vector3(0f, 1.04f, 0.66f);
            }
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "ClipOnBar";
            bar.transform.SetParent(root);
            bar.transform.localScale = new Vector3(0.6f, 0.04f, 0.06f);
            bar.transform.localPosition = new Vector3(0f, 1.0f, 0.72f);
            bar.GetComponent<Renderer>().material = mat;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (Application.isPlaying) Object.Destroy(child.gameObject);
                else Object.DestroyImmediate(child.gameObject);
            }
        }

        private static Color PickBikeColor(string bikeId)
        {
            if (string.IsNullOrEmpty(bikeId)) return new Color(0.15f, 0.6f, 0.9f);
            int hash = Mathf.Abs(bikeId.GetHashCode());
            float t = (hash % 1000) / 999f;
            return Color.Lerp(new Color(0.15f, 0.6f, 0.9f), new Color(0.9f, 0.25f, 0.2f), t);
        }
    }
}
