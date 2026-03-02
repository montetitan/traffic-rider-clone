using TrafficRider.Core;
using TrafficRider.Data;
using TrafficRider.Systems;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TrafficRider.UI
{
    public class BikeMenuUIController : MonoBehaviour
    {
        private GameObject _panel;
        private Text _coins;
        private Transform _bikeList;
        private Button _homeBtn;
        private Action _homeAction;
        private Button _prevBtn;
        private Button _nextBtn;
        private Button _actionBtn;
        private Text _bikeInfo;
        private RawImage _previewImage;
        private GaragePreview _preview;
        private List<BikeConfig> _currentBikes = new List<BikeConfig>();
        private int _currentBikeIndex;
        private Coroutine _refreshRoutine;
        private bool _needsRefresh;
        private GameConfig _configLocal;
        private SaveData _saveLocal;
        private BikeSelectionSystem _bikeSystemLocal;
        private Coroutine _ensureRoutine;

        public void Build(Transform parent)
        {
            _panel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 520));
            _panel.name = "BikeMenu";

            _coins = CreateLabelTopLeft(_panel.transform, new Vector2(16, -16), "Coins: 0", 18);
            CreateLabelTopLeft(_panel.transform, new Vector2(16, -44), "Bike Selection", 20);

            _previewImage = CreatePreviewImage(_panel.transform, new Vector2(66, -82), new Vector2(288, 160));
            _preview = gameObject.AddComponent<GaragePreview>();
            _preview.Build(_previewImage, new Vector2(288, 160));

            _prevBtn = CreateButtonTopLeft(_panel.transform, new Vector2(16, -250), new Vector2(80, 30), "< Prev");
            _nextBtn = CreateButtonTopLeft(_panel.transform, new Vector2(324, -250), new Vector2(80, 30), "Next >");
            _bikeInfo = CreateLabelTopLeft(_panel.transform, new Vector2(16, -286), "Bike", 14);
            _bikeInfo.rectTransform.sizeDelta = new Vector2(388, 56);
            _actionBtn = CreateButtonTopLeft(_panel.transform, new Vector2(16, -346), new Vector2(388, 32), "Select");

            _bikeList = CreateScrollArea(_panel.transform, new Vector2(16, -90), new Vector2(388, 340)).transform;
            _bikeList.parent.gameObject.SetActive(false);

            _homeBtn = CreateButtonTopLeft(_panel.transform, new Vector2(280, -456), new Vector2(120, 32), "Back");
            _homeBtn.onClick.AddListener(() =>
            {
                if (_homeAction != null)
                {
                    _homeAction();
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                }
            });
            _prevBtn.onClick.AddListener(() =>
            {
                if (_currentBikes == null || _currentBikes.Count == 0) return;
                _currentBikeIndex = (_currentBikeIndex - 1 + _currentBikes.Count) % _currentBikes.Count;
                Refresh();
            });
            _nextBtn.onClick.AddListener(() =>
            {
                if (_currentBikes == null || _currentBikes.Count == 0) return;
                _currentBikeIndex = (_currentBikeIndex + 1) % _currentBikes.Count;
                Refresh();
            });

            Hide();
            StartEnsureLoad();
        }

        public void SetHomeAction(Action action)
        {
            _homeAction = action;
        }

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
            }
            _refreshRoutine = StartCoroutine(RefreshWhenReady());
            _needsRefresh = true;
            StartEnsureLoad();
        }

        public void Hide()
        {
            if (_panel == null) return;
            _panel.SetActive(false);
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
                _refreshRoutine = null;
            }
            _needsRefresh = false;
            if (_ensureRoutine != null)
            {
                StopCoroutine(_ensureRoutine);
                _ensureRoutine = null;
            }
        }

        private void StartEnsureLoad()
        {
            if (_ensureRoutine != null) return;

            // Best-effort immediate local read (works in editor/desktop and avoids empty menus).
            LoadLocalConfigSync();
            if (_configLocal != null)
            {
                RefreshLocal();
            }

            if (!Application.isPlaying)
            {
                return;
            }
            _ensureRoutine = StartCoroutine(EnsurePopulated());
        }

        private System.Collections.IEnumerator EnsurePopulated()
        {
            yield return null;
            if (_bikeList != null && _bikeList.childCount > 0)
            {
                _ensureRoutine = null;
                yield break;
            }
            yield return LoadLocalConfig();
            RefreshLocal();
            _ensureRoutine = null;
        }

        private void Update()
        {
            if (!_needsRefresh || _panel == null || !_panel.activeSelf) return;
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Config != null && gm.Save != null && gm.BikeSelectionSystem != null)
            {
                _needsRefresh = false;
                Refresh();
            }
        }

        private System.Collections.IEnumerator RefreshWhenReady()
        {
            GameManager gm = null;
            float timer = 0f;
            while (timer < 3f)
            {
                gm = GameManager.Instance;
                if (gm == null)
                {
                    GameObject gmObj = new GameObject("GameManager");
                    gm = gmObj.AddComponent<GameManager>();
                    gm.InitializeMenuOnly();
                }
                if (gm != null && gm.Config != null && gm.Save != null && gm.BikeSelectionSystem != null)
                {
                    break;
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            if (gm != null && gm.Config != null && gm.Save != null && gm.BikeSelectionSystem != null)
            {
                Refresh();
            }
            else
            {
                yield return LoadLocalConfig();
                RefreshLocal();
            }
        }

        public void Refresh()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Config == null || gm.Save == null)
            {
                StartEnsureLoad();
                if (_bikeSystemLocal != null && _saveLocal != null)
                {
                    RefreshLocal();
                }
                else
                {
                    Debug.LogWarning("BikeMenu: GameManager/config not ready");
                }
                return;
            }

            _coins.text = "Coins: " + gm.Save.coins;
            if (gm.BikeSelectionSystem == null || gm.Config.bikes == null || gm.Config.bikes.Count == 0)
            {
                if (_bikeInfo != null) _bikeInfo.text = "No bikes found.";
                if (_actionBtn != null) _actionBtn.interactable = false;
                Debug.LogWarning("BikeMenu: no bikes in config");
                return;
            }
            _currentBikes = gm.BikeSelectionSystem.GetBikes().ToList();
            int selectedIdx = _currentBikes.FindIndex(b => b.id == gm.Save.selectedBikeId);
            if (selectedIdx >= 0 && (_currentBikeIndex < 0 || _currentBikeIndex >= _currentBikes.Count))
            {
                _currentBikeIndex = selectedIdx;
            }
            _currentBikeIndex = Mathf.Clamp(_currentBikeIndex, 0, Mathf.Max(0, _currentBikes.Count - 1));
            ShowCurrentBike(
                _currentBikes[_currentBikeIndex],
                gm.Save.coins,
                id => gm.BikeSelectionSystem.IsOwned(id),
                gm.Save.selectedBikeId,
                id =>
                {
                    if (!gm.BikeSelectionSystem.IsOwned(id))
                    {
                        if (!gm.BikeSelectionSystem.TryBuyBike(id)) return;
                    }
                    gm.BikeSelectionSystem.SelectBike(id);
                    gm.RefreshBikeStats();
                    MenuBackdrop backdrop = FindObjectOfType<MenuBackdrop>();
                    if (backdrop != null) backdrop.SetBike(id, gm.Config);
                    Refresh();
                });
        }

        private System.Collections.IEnumerator LoadLocalConfig()
        {
            if (_configLocal != null && _bikeSystemLocal != null) yield break;
            ConfigLoader loader = gameObject.AddComponent<ConfigLoader>();
            yield return loader.LoadConfig(config => _configLocal = config);
            if (Application.isPlaying)
            {
                Destroy(loader);
            }
            else
            {
                DestroyImmediate(loader);
            }
            if (_configLocal == null) yield break;
            _saveLocal = SaveSystem.Load(_configLocal);
            _bikeSystemLocal = new BikeSelectionSystem(_configLocal, _saveLocal);
        }

        private void LoadLocalConfigSync()
        {
            if (_configLocal != null && _bikeSystemLocal != null) return;
            string path = Path.Combine(Application.streamingAssetsPath, ConfigLoader.ConfigFileName);
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            _configLocal = JsonUtility.FromJson<GameConfig>(json);
            if (_configLocal == null) return;
            _saveLocal = SaveSystem.Load(_configLocal);
            _bikeSystemLocal = new BikeSelectionSystem(_configLocal, _saveLocal);
        }

        private void RefreshLocal()
        {
            if (_configLocal == null || _saveLocal == null || _bikeSystemLocal == null) return;
            _coins.text = "Coins: " + _saveLocal.coins;
            _currentBikes = _bikeSystemLocal.GetBikes().ToList();
            if (_currentBikes.Count == 0)
            {
                if (_bikeInfo != null) _bikeInfo.text = "No bikes found.";
                if (_actionBtn != null) _actionBtn.interactable = false;
                return;
            }
            int selectedIdx = _currentBikes.FindIndex(b => b.id == _saveLocal.selectedBikeId);
            if (selectedIdx >= 0 && (_currentBikeIndex < 0 || _currentBikeIndex >= _currentBikes.Count))
            {
                _currentBikeIndex = selectedIdx;
            }
            _currentBikeIndex = Mathf.Clamp(_currentBikeIndex, 0, Mathf.Max(0, _currentBikes.Count - 1));
            ShowCurrentBike(
                _currentBikes[_currentBikeIndex],
                _saveLocal.coins,
                id => _bikeSystemLocal.IsOwned(id),
                _saveLocal.selectedBikeId,
                id =>
                {
                    if (!_bikeSystemLocal.IsOwned(id))
                    {
                        if (!_bikeSystemLocal.TryBuyBike(id)) return;
                    }
                    _bikeSystemLocal.SelectBike(id);
                    SaveSystem.Save(_saveLocal);
                    RefreshLocal();
                });
        }

        private void ShowCurrentBike(BikeConfig bike, int coins, Func<string, bool> isOwned, string selectedBikeId, Action<string> onAction)
        {
            if (bike == null) return;
            bool owned = isOwned != null && isOwned(bike.id);
            bool selected = selectedBikeId == bike.id;
            bool affordable = coins >= bike.price;

            if (_preview != null)
            {
                _preview.ShowBike(bike, owned ? 1f : 0.2f);
            }

            if (_bikeInfo != null)
            {
                string status = owned ? (selected ? "Selected" : "Unlocked") : "Locked";
                _bikeInfo.text = bike.name + " | Speed " + bike.maxSpeed + "\nPrice: " + bike.price + " coins | " + status;
            }

            if (_actionBtn != null)
            {
                Text actionText = _actionBtn.GetComponentInChildren<Text>();
                bool enabled = true;
                string text;
                if (owned)
                {
                    text = selected ? "Selected" : "Select";
                }
                else if (affordable)
                {
                    text = "Unlock (" + bike.price + " coins)";
                }
                else
                {
                    text = "Need " + bike.price + " coins";
                    enabled = false;
                }
                if (actionText != null) actionText.text = text;
                _actionBtn.interactable = enabled;
                _actionBtn.onClick.RemoveAllListeners();
                if (enabled && onAction != null)
                {
                    _actionBtn.onClick.AddListener(() => onAction(bike.id));
                }
            }
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private GameObject CreatePanel(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(parent);
            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.75f);
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return panelObj;
        }

        private Text CreateLabel(Transform parent, Vector2 anchoredPos, string text, int fontSize, TextAnchor anchor)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.color = new Color(0.9f, 0.9f, 0.9f);
            label.alignment = anchor;
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(360, 30);
            label.text = text;
            return label;
        }

        private Text CreateLabelTopLeft(Transform parent, Vector2 anchoredPos, string text, int fontSize)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.color = new Color(0.9f, 0.9f, 0.9f);
            label.alignment = TextAnchor.UpperLeft;
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(360, 24);
            label.text = text;
            return label;
        }

        private Transform CreateScrollArea(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject root = new GameObject("ScrollRoot");
            root.transform.SetParent(parent);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0, 1);
            rootRect.anchorMax = new Vector2(0, 1);
            rootRect.pivot = new Vector2(0, 1);
            rootRect.anchoredPosition = anchoredPos;
            rootRect.sizeDelta = size;
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.6f);

            GameObject content = new GameObject("Content");
            content.transform.SetParent(root.transform);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = size;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 6;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return content.transform;
        }

        private RawImage CreatePreviewImage(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject("BikePreview");
            go.transform.SetParent(parent);
            RawImage image = go.AddComponent<RawImage>();
            image.color = Color.white;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return image;
        }

        private Button CreateButton(Transform parent, Vector2 anchoredPos, Vector2 size, string label)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private Button CreateButtonTopLeft(Transform parent, Vector2 anchoredPos, Vector2 size, string label)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360, 28);
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 28f;
            layout.minHeight = 28f;
            layout.flexibleHeight = 0f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            return button;
        }

        private Button CreateBikeRow(Transform parent, string label, string actionLabel, bool actionEnabled)
        {
            GameObject rowObj = new GameObject("BikeRow");
            rowObj.transform.SetParent(parent);
            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(360, 36);
            LayoutElement rowLayout = rowObj.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 36f;
            rowLayout.minHeight = 36f;
            rowLayout.flexibleHeight = 0f;

            HorizontalLayoutGroup h = rowObj.AddComponent<HorizontalLayoutGroup>();
            h.childControlWidth = false;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            h.padding = new RectOffset(8, 8, 4, 4);
            h.spacing = 8f;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform);
            Text text = labelObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 13;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1f;
            labelLayout.minWidth = 220f;

            Button actionBtn = CreateActionButton(rowObj.transform, actionLabel, actionEnabled);
            return actionBtn;
        }

        private Button CreateActionButton(Transform parent, string label, bool enabled)
        {
            GameObject buttonObj = new GameObject(label + "ActionButton");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = enabled ? new Color(0.25f, 0.25f, 0.25f, 0.95f) : new Color(0.2f, 0.2f, 0.2f, 0.6f);

            Button button = buttonObj.AddComponent<Button>();
            button.interactable = enabled;
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 100f;
            layout.minWidth = 100f;
            layout.preferredHeight = 28f;
            layout.minHeight = 28f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.color = enabled ? new Color(0.95f, 0.95f, 0.95f) : new Color(0.75f, 0.75f, 0.75f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
