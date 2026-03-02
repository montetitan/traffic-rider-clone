using TrafficRider.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class GaragePreview : MonoBehaviour
    {
        private Camera _camera;
        private RenderTexture _rt;
        private GameObject _root;
        private GameObject _bike;

        public void Build(RawImage target, Vector2 size)
        {
            _rt = new RenderTexture((int)size.x, (int)size.y, 16, RenderTextureFormat.ARGB32);
            _rt.Create();
            target.texture = _rt;

            _root = new GameObject("GaragePreviewRoot");
            _root.transform.position = new Vector3(1000f, 1000f, 1000f);

            GameObject camObj = new GameObject("GaragePreviewCamera");
            camObj.transform.SetParent(_root.transform);
            _camera = camObj.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            _camera.targetTexture = _rt;
            _camera.orthographic = false;
            _camera.fieldOfView = 35f;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 20f;
            _camera.transform.localPosition = new Vector3(0f, 1.4f, -3.5f);
            _camera.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

            Light light = _root.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.localRotation = Quaternion.Euler(50f, -20f, 0f);
        }

        public void ShowBike(BikeConfig bike)
        {
            if (_root == null) return;
            if (_bike != null)
            {
                Destroy(_bike);
            }
            _bike = BuildBikeModel(bike);
            _bike.transform.SetParent(_root.transform);
            _bike.transform.localPosition = Vector3.zero;
            _bike.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            ApplyOpacity(1f);
        }

        public void ShowBike(BikeConfig bike, float alpha)
        {
            ShowBike(bike);
            ApplyOpacity(alpha);
        }

        private GameObject BuildBikeModel(BikeConfig bike)
        {
            GameObject bikeRoot = new GameObject("PreviewBike");
            string id = bike != null ? bike.id : "bike_basic";
            TrafficRider.Gameplay.BikeVisual.BuildForBike(bikeRoot.transform, id, PickColor(bike));

            return bikeRoot;
        }

        private static Color PickColor(BikeConfig bike)
        {
            if (bike == null) return new Color(0.2f, 0.7f, 0.9f);
            if (bike.id.Contains("sport")) return new Color(0.9f, 0.3f, 0.1f);
            if (bike.id.Contains("super")) return new Color(0.9f, 0.8f, 0.1f);
            return new Color(0.2f, 0.7f, 0.9f);
        }

        public void ApplyOpacity(float alpha)
        {
            if (_bike == null) return;
            float a = Mathf.Clamp01(alpha);
            Renderer[] renderers = _bike.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.material == null) continue;
                Material m = renderer.material;
                Color c = m.color;
                c.a = a;
                m.color = c;
                SetMaterialBlend(m, a < 0.99f);
            }
        }

        private static void SetMaterialBlend(Material mat, bool transparent)
        {
            if (mat == null) return;
            if (transparent)
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetFloat("_Mode", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        }
    }
}
