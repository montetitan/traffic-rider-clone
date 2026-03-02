using System;
using TrafficRider.Core;
using TrafficRider.Data;
using TrafficRider.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class SettingsUIController : MonoBehaviour
    {
        private GameObject _panel;
        private Image _dimOverlay;
        private Text _coins;
        private Text _topScore;
        private Text _syncLocal;
        private Text _syncGlobal;
        private Text _syncNetwork;
        private Transform _bikeList;
        private Button _twoWayBtn;
        private Button _oneWayBtn;
        private Button _missionsBtn;
        private Button _endlessBtn;
        private Button _orientAutoBtn;
        private Button _orientLandscapeBtn;
        private Button _orientPortraitBtn;
        private GameObject _bikeSectionRoot;
        private Button _bgCityBtn;
        private Button _bgDesertBtn;
        private Button _bgGreenBtn;
        private Button _bgWasteBtn;
        private Text _qualityLabel;
        private Button _qualityAutoBtn;
        private Button _qualityLowBtn;
        private Button _qualityMidBtn;
        private Button _qualityHighBtn;
        private Text _missionsList;
        private Transform _missionChoices;
        private CanvasGroup _canvasGroup;
        private Button _homeBtn;
        private Action _homeAction;
        private bool _showQualitySection;

        public void Build(Transform parent)
        {
            _panel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 520));
            _panel.name = "SettingsPanel";
            _canvasGroup = _panel.AddComponent<CanvasGroup>();
            _dimOverlay = CreateDimOverlay(parent);

            _coins = CreateLabel(_panel.transform, new Vector2(16, -16), "Coins: 0", 18, TextAnchor.UpperLeft);
            _topScore = CreateLabel(_panel.transform, new Vector2(16, -46), "Top: 0 km (GPlay_User)", 16, TextAnchor.UpperLeft);

            CreateLabel(_panel.transform, new Vector2(16, -78), "Sync Status", 16, TextAnchor.UpperLeft);
            _syncLocal = CreateLabel(_panel.transform, new Vector2(16, -100), "Local: OK", 14, TextAnchor.UpperLeft);
            _syncGlobal = CreateLabel(_panel.transform, new Vector2(16, -120), "Global: Not Linked", 14, TextAnchor.UpperLeft);
            _syncNetwork = CreateLabel(_panel.transform, new Vector2(16, -140), "Network: Offline", 14, TextAnchor.UpperLeft);

            Text trafficLabel = CreateLabel(_panel.transform, new Vector2(16, -170), "Traffic", 16, TextAnchor.UpperLeft);
            _twoWayBtn = CreateButton(_panel.transform, new Vector2(16, -196), new Vector2(120, 28), "Two-Way");
            _oneWayBtn = CreateButton(_panel.transform, new Vector2(150, -196), new Vector2(120, 28), "One-Way");
            trafficLabel.gameObject.SetActive(false);
            _twoWayBtn.gameObject.SetActive(false);
            _oneWayBtn.gameObject.SetActive(false);

            // Modes moved under New Game selection panel
            Text modesLabel = CreateLabel(_panel.transform, new Vector2(16, -230), "Modes", 16, TextAnchor.UpperLeft);
            _endlessBtn = CreateButton(_panel.transform, new Vector2(16, -256), new Vector2(120, 28), "Endless");
            _missionsBtn = CreateButton(_panel.transform, new Vector2(150, -256), new Vector2(120, 28), "Missions");
            modesLabel.gameObject.SetActive(false);
            _endlessBtn.gameObject.SetActive(false);
            _missionsBtn.gameObject.SetActive(false);

            CreateLabel(_panel.transform, new Vector2(16, -288), "Orientation", 16, TextAnchor.UpperLeft);
            _orientAutoBtn = CreateButton(_panel.transform, new Vector2(16, -314), new Vector2(90, 28), "Auto");
            _orientLandscapeBtn = CreateButton(_panel.transform, new Vector2(112, -314), new Vector2(90, 28), "Land");
            _orientPortraitBtn = CreateButton(_panel.transform, new Vector2(208, -314), new Vector2(90, 28), "Port");

            _bikeSectionRoot = new GameObject("BikeSection");
            _bikeSectionRoot.transform.SetParent(_panel.transform);

            CreateLabel(_panel.transform, new Vector2(16, -348), "Background", 16, TextAnchor.UpperLeft);
            _bgCityBtn = CreateButton(_panel.transform, new Vector2(16, -374), new Vector2(90, 28), "City");
            _bgDesertBtn = CreateButton(_panel.transform, new Vector2(112, -374), new Vector2(90, 28), "Desert");
            _bgGreenBtn = CreateButton(_panel.transform, new Vector2(208, -374), new Vector2(90, 28), "Green");
            _bgWasteBtn = CreateButton(_panel.transform, new Vector2(304, -374), new Vector2(90, 28), "Waste");

            CreateLabel(_panel.transform, new Vector2(16, -408), "Missions", 16, TextAnchor.UpperLeft);
            _missionsList = CreateLabel(_panel.transform, new Vector2(16, -434), "Missions", 12, TextAnchor.UpperLeft);
            _missionsList.rectTransform.sizeDelta = new Vector2(388, 90);
            _missionChoices = CreateScrollArea(_panel.transform, new Vector2(16, -530), new Vector2(388, 120)).transform;

            _qualityLabel = CreateLabel(_panel.transform, new Vector2(16, -230), "Quality", 16, TextAnchor.UpperLeft);
            _qualityAutoBtn = CreateButton(_panel.transform, new Vector2(16, -256), new Vector2(90, 28), "Auto");
            _qualityLowBtn = CreateButton(_panel.transform, new Vector2(112, -256), new Vector2(90, 28), "Low");
            _qualityMidBtn = CreateButton(_panel.transform, new Vector2(208, -256), new Vector2(90, 28), "Mid");
            _qualityHighBtn = CreateButton(_panel.transform, new Vector2(304, -256), new Vector2(90, 28), "High");

            _homeBtn = CreateButton(_panel.transform, new Vector2(304, -20), new Vector2(90, 28), "Back");

            CreateLabel(_bikeSectionRoot.transform, new Vector2(16, -408), "Bikes", 16, TextAnchor.UpperLeft);
            _bikeList = CreateScrollArea(_bikeSectionRoot.transform, new Vector2(16, -434), new Vector2(388, 120)).transform;

            HookEvents();
            SetShowQualitySection(false);
            Hide();
        }

        private void HookEvents()
        {
            _twoWayBtn.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetTrafficMode(true);
                Refresh();
            });
            _oneWayBtn.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetTrafficMode(false);
                Refresh();
            });
            _endlessBtn.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetMode(GameMode.Endless);
                Refresh();
            });
            _missionsBtn.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetMode(GameMode.Missions);
                Refresh();
            });

            _orientAutoBtn.onClick.AddListener(() => { GameManager.Instance?.SetOrientation("auto"); Refresh(); });
            _orientLandscapeBtn.onClick.AddListener(() => { GameManager.Instance?.SetOrientation("landscape"); Refresh(); });
            _orientPortraitBtn.onClick.AddListener(() => { GameManager.Instance?.SetOrientation("portrait"); Refresh(); });

            _bgCityBtn.onClick.AddListener(() => { GameManager.Instance?.SetBackground("city"); Refresh(); });
            _bgDesertBtn.onClick.AddListener(() => { GameManager.Instance?.SetBackground("desert"); Refresh(); });
            _bgGreenBtn.onClick.AddListener(() => { GameManager.Instance?.SetBackground("greenery"); Refresh(); });
            _bgWasteBtn.onClick.AddListener(() => { GameManager.Instance?.SetBackground("wasteland"); Refresh(); });
            _qualityAutoBtn.onClick.AddListener(() => { GameManager.Instance?.SetQuality("auto"); Refresh(); });
            _qualityLowBtn.onClick.AddListener(() => { GameManager.Instance?.SetQuality("low"); Refresh(); });
            _qualityMidBtn.onClick.AddListener(() => { GameManager.Instance?.SetQuality("mid"); Refresh(); });
            _qualityHighBtn.onClick.AddListener(() => { GameManager.Instance?.SetQuality("high"); Refresh(); });

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
        }

        public void SetHomeAction(Action action)
        {
            _homeAction = action;
        }

        public void SetShowQualitySection(bool show)
        {
            _showQualitySection = show;
            if (_qualityLabel != null) _qualityLabel.gameObject.SetActive(show);
            if (_qualityAutoBtn != null) _qualityAutoBtn.gameObject.SetActive(show);
            if (_qualityLowBtn != null) _qualityLowBtn.gameObject.SetActive(show);
            if (_qualityMidBtn != null) _qualityMidBtn.gameObject.SetActive(show);
            if (_qualityHighBtn != null) _qualityHighBtn.gameObject.SetActive(show);
        }

        public void Toggle()
        {
            if (_panel == null) return;
            bool show = !_panel.activeSelf;
            _panel.SetActive(show);
            if (show) Refresh();
        }

        public void Show()
        {
            if (_panel == null) return;
            _panel.transform.SetAsLastSibling();
            if (_dimOverlay != null)
            {
                _dimOverlay.gameObject.SetActive(true);
                _dimOverlay.transform.SetAsLastSibling();
                _panel.transform.SetAsLastSibling();
            }
            _panel.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
            Refresh();
        }

        public void Hide()
        {
            if (_panel == null) return;
            _panel.SetActive(false);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            if (_dimOverlay != null)
            {
                _dimOverlay.gameObject.SetActive(false);
            }
        }

        public bool IsVisible()
        {
            return _panel != null && _panel.activeSelf;
        }

        public void Refresh()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Config == null || gm.Save == null) return;

            _coins.text = "Coins: " + gm.Save.coins;
            SetTopScore(gm.Save.topScoreKm, gm.Save.googlePlayId);

            _twoWayBtn.GetComponentInChildren<Text>().text = gm.Save.twoWayTraffic ? "Two-Way (On)" : "Two-Way";
            _oneWayBtn.GetComponentInChildren<Text>().text = gm.Save.twoWayTraffic ? "One-Way" : "One-Way (On)";

            _endlessBtn.GetComponentInChildren<Text>().text = gm.mode == GameMode.Endless ? "Endless (On)" : "Endless";
            _missionsBtn.GetComponentInChildren<Text>().text = gm.mode == GameMode.Missions ? "Missions (On)" : "Missions";

            if (gm.Save.orientation == "landscape")
            {
                _orientLandscapeBtn.GetComponentInChildren<Text>().text = "Land (On)";
                _orientPortraitBtn.GetComponentInChildren<Text>().text = "Port";
                _orientAutoBtn.GetComponentInChildren<Text>().text = "Auto";
            }
            else if (gm.Save.orientation == "portrait")
            {
                _orientLandscapeBtn.GetComponentInChildren<Text>().text = "Land";
                _orientPortraitBtn.GetComponentInChildren<Text>().text = "Port (On)";
                _orientAutoBtn.GetComponentInChildren<Text>().text = "Auto";
            }
            else
            {
                _orientLandscapeBtn.GetComponentInChildren<Text>().text = "Land";
                _orientPortraitBtn.GetComponentInChildren<Text>().text = "Port";
                _orientAutoBtn.GetComponentInChildren<Text>().text = "Auto (On)";
            }

            string bg = gm.Save.background;
            _bgCityBtn.GetComponentInChildren<Text>().text = bg == "city" ? "City (On)" : "City";
            _bgDesertBtn.GetComponentInChildren<Text>().text = bg == "desert" ? "Desert (On)" : "Desert";
            _bgGreenBtn.GetComponentInChildren<Text>().text = bg == "greenery" ? "Green (On)" : "Green";
            _bgWasteBtn.GetComponentInChildren<Text>().text = bg == "wasteland" ? "Waste (On)" : "Waste";
            SetShowQualitySection(_showQualitySection);

            string quality = string.IsNullOrEmpty(gm.Save.quality) ? "auto" : gm.Save.quality;
            _qualityAutoBtn.GetComponentInChildren<Text>().text = quality == "auto" ? "Auto (On)" : "Auto";
            _qualityLowBtn.GetComponentInChildren<Text>().text = quality == "low" ? "Low (On)" : "Low";
            _qualityMidBtn.GetComponentInChildren<Text>().text = quality == "mid" ? "Mid (On)" : "Mid";
            _qualityHighBtn.GetComponentInChildren<Text>().text = quality == "high" ? "High (On)" : "High";

            if (_missionsList != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var progress in gm.MissionSystem.GetProgress())
                {
                    var cfg = gm.MissionSystem.GetConfig(progress.id);
                    if (cfg == null) continue;
                    string mark = progress.completed ? "[x] " : "[ ] ";
                    sb.Append(mark);
                    sb.Append(cfg.name);
                    sb.Append(" ");
                    sb.Append(progress.value);
                    sb.Append("/");
                    sb.Append(cfg.target);
                    sb.AppendLine();
                }
                _missionsList.text = sb.ToString();
            }

            // Mission selection only in Missions mode
            if (_missionChoices != null)
            {
                ClearChildren(_missionChoices);
                if (gm.mode == GameMode.Missions)
                {
                    foreach (var progress in gm.MissionSystem.GetProgress())
                    {
                        var cfg = gm.MissionSystem.GetConfig(progress.id);
                        if (cfg == null) continue;
                        string mark = progress.completed ? "[x] " : "[ ] ";
                        string label = mark + cfg.name;
                        Button btn = CreateButton(_missionChoices, label);
                        btn.onClick.AddListener(() =>
                        {
                            gm.Save.selectedMissionId = cfg.id;
                            SaveSystem.Save(gm.Save);
                            Refresh();
                        });
                    }
                }
            }

            ClearChildren(_bikeList);
            foreach (BikeConfig bike in gm.BikeSelectionSystem.GetBikes())
            {
                BikeConfig localBike = bike;
                bool owned = gm.BikeSelectionSystem.IsOwned(localBike.id);
                string label = localBike.name + " | Speed " + localBike.maxSpeed + " | $" + localBike.price;
                if (owned && gm.Save.selectedBikeId == localBike.id)
                {
                    label = localBike.name + " (Selected)";
                }

                Button btn = CreateButton(_bikeList, label);
                btn.onClick.AddListener(() =>
                {
                    if (!owned)
                    {
                        if (!gm.BikeSelectionSystem.TryBuyBike(localBike.id))
                        {
                            return;
                        }
                    }
                    gm.BikeSelectionSystem.SelectBike(localBike.id);
                    gm.RefreshBikeStats();
                    Refresh();
                });
            }
        }

        public void ShowBikeSection(bool show)
        {
            if (_bikeSectionRoot != null)
            {
                _bikeSectionRoot.SetActive(show);
            }
            if (show)
            {
                Refresh();
            }
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

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            return button;
        }

        public void SetTopScore(float km, string userId)
        {
            if (_topScore == null) return;
            _topScore.text = "Top: " + km.ToString("F2") + " km (" + userId + ")";
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
            image.color = new Color(0f, 0f, 0f, 0.85f);
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return panelObj;
        }

        private Image CreateDimOverlay(Transform parent)
        {
            GameObject overlayObj = new GameObject("SettingsDimOverlay");
            overlayObj.transform.SetParent(parent);
            Image image = overlayObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.45f);
            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            overlayObj.SetActive(false);
            return image;
        }

        private Text CreateLabel(Transform parent, Vector2 anchoredPos, string text, int fontSize, TextAnchor anchor)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.color = new Color(0.85f, 0.85f, 0.85f);
            label.alignment = anchor;
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

        private Button CreateButton(Transform parent, Vector2 anchoredPos, Vector2 size, string label)
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
    }
}
