using System.Collections.Generic;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class RoadSpawner : MonoBehaviour
    {
        public Transform player;
        public int segmentsAhead = 8;
        public int segmentsBehind = 2;
        public float segmentLength = 40f;
        public float roadWidth = 20f;
        public int lanes = 6;
        public bool twoWayCenterDivider = true;

        private readonly List<GameObject> _segments = new List<GameObject>();
        private float _lastSpawnZ;

        private Material _roadMaterial;
        private Material _laneMaterial;
        private Material _dividerMaterial;
        private Material _railRed;
        private Material _railYellow;

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            // Longer segments + more look-ahead reduce runtime build spikes and extend draw distance.
            segmentLength = Mathf.Max(segmentLength, 60f);
            segmentsAhead = Mathf.Max(segmentsAhead, 14);
            segmentsBehind = Mathf.Max(segmentsBehind, 3);
            _lastSpawnZ = Mathf.Floor(player.position.z / segmentLength) * segmentLength;
            CreateInitialSegments();
        }

        public void SetTrafficDirection(bool twoWay)
        {
            twoWayCenterDivider = twoWay;
            if (player == null) return;
            ClearAll();
            _lastSpawnZ = Mathf.Floor(player.position.z / segmentLength) * segmentLength;
            CreateInitialSegments();
        }

        private void Update()
        {
            if (player == null) return;

            float playerSegmentZ = Mathf.Floor(player.position.z / segmentLength) * segmentLength;
            while (_lastSpawnZ < playerSegmentZ + segmentsAhead * segmentLength)
            {
                _lastSpawnZ += segmentLength;
                CreateSegment(_lastSpawnZ);
            }

            CleanupSegments(player.position.z - segmentsBehind * segmentLength);
        }

        private void CreateInitialSegments()
        {
            for (int i = -segmentsBehind; i <= segmentsAhead; i++)
            {
                float z = _lastSpawnZ + i * segmentLength;
                CreateSegment(z);
            }
        }

        private void CreateSegment(float z)
        {
            EnsureMaterials();

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "RoadSegment_" + z.ToString("F0");
            segment.transform.SetParent(transform);
            segment.transform.position = new Vector3(0f, -0.5f, z + segmentLength * 0.5f);
            segment.transform.localScale = new Vector3(roadWidth, 0.6f, segmentLength);
            segment.GetComponent<Renderer>().material = _roadMaterial;

            CreateLaneLines(segment.transform);

            _segments.Add(segment);

            float edgeOffset = 0.25f;
            CreateGuardRail(segment.transform, -roadWidth * 0.5f - edgeOffset, z, _railRed);
            CreateGuardRail(segment.transform, roadWidth * 0.5f + edgeOffset, z, _railYellow);
        }

        private void EnsureMaterials()
        {
            if (_roadMaterial == null)
            {
                _roadMaterial = BuildMat(new Color(0.2f, 0.2f, 0.2f));
            }
            if (_laneMaterial == null)
            {
                _laneMaterial = BuildMat(new Color(0.45f, 0.35f, 0.08f));
            }
            if (_dividerMaterial == null)
            {
                _dividerMaterial = BuildMat(new Color(0.65f, 0.3f, 0.05f));
            }
            if (_railRed == null)
            {
                _railRed = BuildMat(new Color(0.55f, 0.12f, 0.12f));
            }
            if (_railYellow == null)
            {
                _railYellow = BuildMat(new Color(0.55f, 0.5f, 0.1f));
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

        private void CreateLaneLines(Transform parent)
        {
            float laneWidth = roadWidth / lanes;

            if (twoWayCenterDivider)
            {
                // Two-way: small solid double center line
                CreateCenterSolid(parent, -0.05f);
                CreateCenterSolid(parent, 0.05f);
            }
            else
            {
                // One-way: center dashed line
                float dashLength = Mathf.Max(0.05f, laneWidth * 0.01f);
                int dashCount = Mathf.Clamp(Mathf.CeilToInt(segmentLength / (dashLength * 3.6f)), 16, 36);
                for (int i = 0; i < dashCount; i++)
                {
                    float z = parent.position.z - segmentLength * 0.45f + i * dashLength * 1.8f + dashLength * 0.5f;
                    CreateCenterDash(parent, new Vector3(0f, -0.18f, z), dashLength);
                }
            }

            // dashed lane dividers between each lane
            float laneDashLength = Mathf.Max(0.05f, laneWidth * 0.01f);
            int laneDashCount = Mathf.Clamp(Mathf.CeilToInt(segmentLength / (laneDashLength * 3.6f)), 16, 36);
            for (int lane = 1; lane < lanes; lane++)
            {
                if (lane == lanes / 2) continue;
                float x = -roadWidth * 0.5f + laneWidth * lane;
                for (int i = 0; i < laneDashCount; i++)
                {
                    float z = parent.position.z - segmentLength * 0.45f + i * laneDashLength * 1.8f + laneDashLength * 0.5f;
                    GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    dash.name = "LaneDash";
                    dash.transform.SetParent(parent);
                    dash.transform.position = new Vector3(x, -0.18f, z);
                    dash.transform.localScale = new Vector3(0.012f, 0.01f, laneDashLength);
                    dash.GetComponent<Renderer>().material = _laneMaterial;
                    Destroy(dash.GetComponent<Collider>());
                }
            }
        }

        private void CreateGuardRail(Transform parent, float x, float z, Material mat)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "GuardRail";
            rail.transform.SetParent(parent);
            rail.transform.position = new Vector3(x, 0.05f, z + segmentLength * 0.5f);
            rail.transform.localScale = new Vector3(0.03f, 0.2f, segmentLength);
            Renderer renderer = rail.GetComponent<Renderer>();
            renderer.material = mat;
            BoxCollider col = rail.GetComponent<BoxCollider>();
            col.isTrigger = false;
        }

        private void CreateCenterDash(Transform parent, Vector3 pos, float length)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = "CenterDash";
            dash.transform.SetParent(parent);
            dash.transform.position = pos;
            dash.transform.localScale = new Vector3(0.012f, 0.01f, length);
            dash.GetComponent<Renderer>().material = _dividerMaterial;
            Destroy(dash.GetComponent<Collider>());
        }

        private void CreateCenterSolid(Transform parent, float x)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "CenterSolid";
            line.transform.SetParent(parent);
            line.transform.position = new Vector3(x, -0.18f, parent.position.z);
            line.transform.localScale = new Vector3(0.012f, 0.01f, segmentLength * 0.9f);
            line.GetComponent<Renderer>().material = _dividerMaterial;
            Destroy(line.GetComponent<Collider>());
        }

        private void CleanupSegments(float minZ)
        {
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                GameObject segment = _segments[i];
                if (segment.transform.position.z + segmentLength < minZ)
                {
                    Destroy(segment);
                    _segments.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                if (_segments[i] != null)
                {
                    Destroy(_segments[i]);
                }
            }
            _segments.Clear();
        }

        private void OnValidate()
        {
            lanes = 6;
            roadWidth = 20f;
        }
    }
}
