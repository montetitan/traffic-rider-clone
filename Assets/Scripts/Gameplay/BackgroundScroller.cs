using System.Collections.Generic;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class BackgroundScroller : MonoBehaviour
    {
        public Transform player;
        public float scrollFactor = 0.18f;
        public float visualSpeedMultiplier = 4.2f; // #2f 4.2f
        public float segmentLength = 180f;
        public int segments = 32;
        public float offsetX = 21f;
        public float groundWidth = 20f;
        public float groundThickness = 0.6f;

        private readonly List<GameObject> _left = new List<GameObject>();
        private readonly List<GameObject> _right = new List<GameObject>();
        private readonly List<GameObject> _groundLeft = new List<GameObject>();
        private readonly List<GameObject> _groundRight = new List<GameObject>();
        private float _baseZStart;

        private enum BackgroundTheme { City, Desert, Greenery, Wasteland }
        private BackgroundTheme _theme = BackgroundTheme.City;

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            // Extend horizon to keep side world visible at high speed.
            segmentLength = Mathf.Max(segmentLength, 220f);
            segments = Mathf.Max(segments, 48);
            _baseZStart = 0f;
            CreateSegments();
        }

        public void SetTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme)) return;
            string t = theme.ToLowerInvariant();
            if (t == "desert") _theme = BackgroundTheme.Desert;
            else if (t == "greenery") _theme = BackgroundTheme.Greenery;
            else if (t == "wasteland") _theme = BackgroundTheme.Wasteland;
            else _theme = BackgroundTheme.City;
            RebuildAll();
        }

        private void Update()
        {
            if (player == null) return;

            float scrollZ = player.position.z * scrollFactor * visualSpeedMultiplier;
            UpdateRows(_left, scrollZ);
            UpdateRows(_right, scrollZ);
            UpdateRows(_groundLeft, scrollZ);
            UpdateRows(_groundRight, scrollZ);
        }

        private void CreateSegments()
        {
            for (int i = 0; i < segments; i++)
            {
                float z = _baseZStart + i * segmentLength;
                _left.Add(CreateBlockRow(new Vector3(-offsetX, 0f, z)));
                _right.Add(CreateBlockRow(new Vector3(offsetX, 0f, z)));
                _groundLeft.Add(CreateGroundStrip(new Vector3(-offsetX, -0.3f, z)));
                _groundRight.Add(CreateGroundStrip(new Vector3(offsetX, -0.3f, z)));
            }
        }

        private GameObject CreateBlockRow(Vector3 pos)
        {
            GameObject row = new GameObject("BackgroundRow");
            row.transform.SetParent(transform);
            row.transform.position = pos;
            row.AddComponent<BackgroundRow>().baseZ = pos.z;

            Color[] palette = GetBuildingPalette();

            for (int i = 0; i < 6; i++)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.SetParent(row.transform);
                block.transform.localScale = new Vector3(4f, Random.Range(4f, 10f), 6f);
                block.transform.localPosition = new Vector3(Random.Range(-2f, 2f), block.transform.localScale.y * 0.5f, i * 10f - 12f);
                Color baseColor = palette[Random.Range(0, palette.Length)];
                float tint = 0.85f + Random.value * 0.2f;
                block.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
                {
                    color = new Color(baseColor.r * tint, baseColor.g * tint, baseColor.b * tint)
                };
            }
            return row;
        }

        private GameObject CreateGroundStrip(Vector3 pos)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "GroundStrip";
            strip.transform.SetParent(transform);
            strip.transform.position = pos;
            strip.transform.localScale = new Vector3(groundWidth, groundThickness, segmentLength);
            strip.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
            {
                color = GetGroundColor()
            };
            strip.AddComponent<BackgroundRow>().baseZ = pos.z;
            Destroy(strip.GetComponent<Collider>());
            return strip;
        }

        private void UpdateRows(List<GameObject> rows, float scrollZ)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                GameObject row = rows[i];
                Vector3 pos = row.transform.position;
                pos.z = scrollZ + GetRowBaseZ(row);
                row.transform.position = pos;
            }

            RecycleIfNeeded(rows, scrollZ);
        }

        private void RecycleIfNeeded(List<GameObject> rows, float scrollZ)
        {
            float loopLength = segments * segmentLength;
            for (int i = 0; i < rows.Count; i++)
            {
                GameObject row = rows[i];
                if (row.transform.position.z + segmentLength < scrollZ - segmentLength)
                {
                    BackgroundRow data = row.GetComponent<BackgroundRow>();
                    if (data != null)
                    {
                        data.baseZ += loopLength;
                        row.transform.position = new Vector3(row.transform.position.x, row.transform.position.y, scrollZ + data.baseZ);
                    }
                }
            }
        }

        private float GetRowBaseZ(GameObject row)
        {
            if (row == null) return 0f;
            BackgroundRow data = row.GetComponent<BackgroundRow>();
            return data != null ? data.baseZ : 0f;
        }

        private void RebuildAll()
        {
            ClearList(_left);
            ClearList(_right);
            ClearList(_groundLeft);
            ClearList(_groundRight);
            CreateSegments();
        }

        private void ClearList(List<GameObject> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] != null)
                {
                    Destroy(list[i]);
                }
            }
            list.Clear();
        }

        private Color[] GetBuildingPalette()
        {
            switch (_theme)
            {
                case BackgroundTheme.Desert:
                    return new[] { new Color(0.45f, 0.35f, 0.22f), new Color(0.55f, 0.45f, 0.3f), new Color(0.6f, 0.5f, 0.35f) };
                case BackgroundTheme.Greenery:
                    return new[] { new Color(0.2f, 0.35f, 0.22f), new Color(0.25f, 0.4f, 0.3f), new Color(0.3f, 0.45f, 0.28f) };
                case BackgroundTheme.Wasteland:
                    return new[] { new Color(0.25f, 0.25f, 0.25f), new Color(0.3f, 0.2f, 0.2f), new Color(0.2f, 0.2f, 0.22f) };
                default:
                    return new[] { new Color(0.25f, 0.3f, 0.35f), new Color(0.3f, 0.25f, 0.22f), new Color(0.28f, 0.24f, 0.32f) };
            }
        }

        private Color GetGroundColor()
        {
            switch (_theme)
            {
                case BackgroundTheme.Desert:
                    return new Color(0.35f, 0.28f, 0.18f);
                case BackgroundTheme.Greenery:
                    return new Color(0.12f, 0.2f, 0.12f);
                case BackgroundTheme.Wasteland:
                    return new Color(0.18f, 0.18f, 0.18f);
                default:
                    return new Color(0.12f, 0.12f, 0.12f);
            }
        }
    }

    public class BackgroundRow : MonoBehaviour
    {
        public float baseZ;
    }
}
