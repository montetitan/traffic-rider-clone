using TrafficRider.Core;
using TrafficRider.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class StartMenuUIController : MonoBehaviour
    {
        private GameObject _panel;
        private Button _newGameBtn;
        private Button _settingsBtn;
        private Button _bikeBtn;
        private Button _quitBtn;
        private Button _buyCoinsBtn;
        private Button _donateBtn;
        private Button _missionsBtn;
        private SettingsUIController _settings;
        private BikeMenuUIController _bikeMenu;
        public string gameplaySceneName = "NewGame";
        public string missionMenuSceneName = "MissionMenu";
        private CanvasGroup _canvasGroup;
        private GameObject _buyCoinsPanel;
        private GameObject _donatePanel;
        private GameObject _infoPanel;
        private Text _infoText;
        private GameObject _aboutPanel;
        private Button _aboutBtn;
        private GameObject _newGamePanel;
        private GameObject _confirmQuitPanel;
        private Button _modeEndlessBtn;
        private Button _modeMissionsBtn;
        private Button _trafficTwoWayBtn;
        private Button _trafficOneWayBtn;
        private Button _startBtn;
        private Transform _missionsListRoot;
        private GameObject _missionsScrollRoot;
        private Coroutine _newGameRefreshRoutine;
        private bool _selectedTwoWay = true;
        private Image _dimOverlay;

        public void Build(Transform parent, SettingsUIController settings)
        {
            _settings = settings;
            _panel = CreatePanel(parent, new Vector2(0, 0), new Vector2(360, 260));
            _panel.name = "StartMenu";
            _canvasGroup = _panel.AddComponent<CanvasGroup>();
            _dimOverlay = CreateDimOverlay(parent);

            CreateLabel(_panel.transform, new Vector2(0, 90), "Traffic Rider", 26);

            _newGameBtn = CreateButton(_panel.transform, new Vector2(0, 30), new Vector2(180, 36), "New Game");
            _settingsBtn = CreateButton(_panel.transform, new Vector2(0, -20), new Vector2(180, 36), "Settings");
            _bikeBtn = CreateButton(_panel.transform, new Vector2(0, -70), new Vector2(180, 36), "Bike Selection");
            _buyCoinsBtn = CreateButton(_panel.transform, new Vector2(0, -120), new Vector2(180, 36), "Buy Coins");
            _missionsBtn = CreateButton(_panel.transform, new Vector2(0, -170), new Vector2(180, 36), "Missions");
            _donateBtn = CreateButton(_panel.transform, new Vector2(0, -220), new Vector2(180, 36), "Donate");
            _aboutBtn = CreateButton(_panel.transform, new Vector2(0, -270), new Vector2(180, 36), "About Me");
            _quitBtn = CreateButton(_panel.transform, new Vector2(0, -320), new Vector2(180, 36), "Quit");

            BuildBuyCoinsPanel(parent);
            BuildDonatePanel(parent);
            BuildInfoPanel(parent);
            BuildAboutPanel(parent);
            BuildBikeMenu(parent);

            _newGameBtn.onClick.AddListener(() =>
            {
                ShowNewGamePanel();
            });

            _settingsBtn.onClick.AddListener(() =>
            {
                if (_settings != null)
                {
                    _settings.SetShowQualitySection(true);
                    _settings.Show();
                    _settings.ShowBikeSection(false);
                    SetInteractable(false);
                }
            });

            _bikeBtn.onClick.AddListener(() =>
            {
                Hide();
                if (_bikeMenu != null)
                {
                    _bikeMenu.Show();
                }
            });

            _buyCoinsBtn.onClick.AddListener(() =>
            {
                ShowBuyCoins();
            });

            _missionsBtn.onClick.AddListener(() =>
            {
                if (Application.CanStreamedLevelBeLoaded(missionMenuSceneName))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(missionMenuSceneName);
                }
                else
                {
                    ShowInfo("MissionMenu scene is not in Build Settings. Use\nTrafficRider > Create Mission Selection Menu Scene\nor add it in File > Build Settings.");
                }
            });

            _donateBtn.onClick.AddListener(() =>
            {
                ShowDonate();
            });

            _aboutBtn.onClick.AddListener(() =>
            {
                ShowAbout();
            });

            _quitBtn.onClick.AddListener(() =>
            {
                ShowConfirmQuit(true);
            });

            BuildNewGamePanel(parent);
            BuildConfirmQuitPanel(parent);
        }

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
            SetDimmed(false);
            GameManager.Instance?.Pause(true);
            Time.timeScale = 0f;
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
            SetDimmed(false);
        }

        public void SetInteractable(bool value)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.blocksRaycasts = value;
            _canvasGroup.interactable = value;
        }

        public void SetDimmed(bool dim)
        {
            if (_dimOverlay == null) return;
            _dimOverlay.gameObject.SetActive(false);
        }

        private void BuildBuyCoinsPanel(Transform parent)
        {
            _buyCoinsPanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 240));
            _buyCoinsPanel.name = "BuyCoinsPanel";

            CreateLabel(_buyCoinsPanel.transform, new Vector2(0, 70), "Buy Coins (Demo)", 20);
            CreateLabel(_buyCoinsPanel.transform, new Vector2(0, 30), "10000 coins - 100 rs", 16);
            CreateLabel(_buyCoinsPanel.transform, new Vector2(0, 0), "Google Pay: montetitan@okicici", 14);
            CreateLabel(_buyCoinsPanel.transform, new Vector2(0, -30), "Placeholder only (no real payment)", 12);

            Button closeBtn = CreateButton(_buyCoinsPanel.transform, new Vector2(-70, -80), new Vector2(140, 32), "Back");
            closeBtn.onClick.AddListener(() =>
            {
                _buyCoinsPanel.SetActive(false);
            });

            Button backToMenuBtn = CreateButton(_buyCoinsPanel.transform, new Vector2(70, -80), new Vector2(140, 32), "Back");
            backToMenuBtn.onClick.AddListener(() =>
            {
                _buyCoinsPanel.SetActive(false);
                Show();
            });

            _buyCoinsPanel.SetActive(false);
        }

        private void BuildNewGamePanel(Transform parent)
        {
            _newGamePanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 320));
            _newGamePanel.name = "NewGamePanel";
            _newGamePanel.AddComponent<CanvasGroup>();

            CreateLabel(_newGamePanel.transform, new Vector2(0, 110), "New Game", 20);
            _modeEndlessBtn = CreateButton(_newGamePanel.transform, new Vector2(-90, 60), new Vector2(160, 32), "Endless");
            _modeMissionsBtn = CreateButton(_newGamePanel.transform, new Vector2(90, 60), new Vector2(160, 32), "Missions");
            _modeMissionsBtn.gameObject.SetActive(false);

            CreateLabel(_newGamePanel.transform, new Vector2(0, 20), "Traffic Type", 16);
            _trafficTwoWayBtn = CreateButton(_newGamePanel.transform, new Vector2(-90, -10), new Vector2(160, 32), "Two-Way");
            _trafficOneWayBtn = CreateButton(_newGamePanel.transform, new Vector2(90, -10), new Vector2(160, 32), "One-Way");

            _missionsListRoot = CreateScrollArea(_newGamePanel.transform, new Vector2(0, -10), new Vector2(360, 90)).transform;
            _missionsScrollRoot = _missionsListRoot.parent != null ? _missionsListRoot.parent.gameObject : _missionsListRoot.gameObject;
            CanvasGroup scrollGroup = _missionsScrollRoot.GetComponent<CanvasGroup>();
            if (scrollGroup == null)
            {
                scrollGroup = _missionsScrollRoot.AddComponent<CanvasGroup>();
            }
            scrollGroup.blocksRaycasts = false;
            Image scrollBg = _missionsScrollRoot.GetComponent<Image>();
            if (scrollBg != null) scrollBg.raycastTarget = false;

            _startBtn = CreateButton(_newGamePanel.transform, new Vector2(0, -110), new Vector2(140, 32), "Start");
            Button cancelBtn = CreateButton(_newGamePanel.transform, new Vector2(0, -150), new Vector2(140, 32), "Cancel");

            _modeEndlessBtn.onClick.AddListener(() =>
            {
                RefreshNewGamePanel();
            });
            _trafficTwoWayBtn.onClick.AddListener(() =>
            {
                _selectedTwoWay = true;
                RefreshNewGamePanel();
            });
            _trafficOneWayBtn.onClick.AddListener(() =>
            {
                _selectedTwoWay = false;
                RefreshNewGamePanel();
            });

            _startBtn.onClick.AddListener(() =>
            {
                GameManager gm = GameManager.Instance;
                if (gm != null)
                {
                    gm.SetMode(GameMode.Endless);
                    gm.SetTrafficMode(_selectedTwoWay);
                }
                Time.timeScale = 1f;
                GameManager.Instance?.Pause(false);
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
            });

            cancelBtn.onClick.AddListener(() =>
            {
                _newGamePanel.SetActive(false);
                if (_canvasGroup != null)
                {
                    _canvasGroup.blocksRaycasts = true;
                    _canvasGroup.interactable = true;
                }
                Show();
            });

            _newGamePanel.SetActive(false);
        }

        private void BuildBikeMenu(Transform parent)
        {
            GameObject obj = new GameObject("BikeMenuController");
            obj.transform.SetParent(parent);
            _bikeMenu = obj.AddComponent<BikeMenuUIController>();
            _bikeMenu.Build(parent);
            _bikeMenu.SetHomeAction(() =>
            {
                _bikeMenu.Hide();
                Show();
            });
        }

        private void ShowBuyCoins()
        {
            if (_buyCoinsPanel == null) return;
            _buyCoinsPanel.SetActive(true);
        }

        private void BuildDonatePanel(Transform parent)
        {
            _donatePanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 240));
            _donatePanel.name = "DonatePanel";

            CreateLabel(_donatePanel.transform, new Vector2(0, 70), "Donate (UPI)", 20);
            CreateLabel(_donatePanel.transform, new Vector2(0, 30), "Google Pay: montetitan@okicici", 14);
            CreateLabel(_donatePanel.transform, new Vector2(0, 5), "Tap Pay Now to open Google Pay", 12);

            Button payBtn = CreateButton(_donatePanel.transform, new Vector2(-70, -80), new Vector2(140, 32), "Pay Now");
            payBtn.onClick.AddListener(OpenDonateUpi);

            Button cancelBtn = CreateButton(_donatePanel.transform, new Vector2(70, -80), new Vector2(140, 32), "Cancel");
            cancelBtn.onClick.AddListener(() =>
            {
                _donatePanel.SetActive(false);
            });

            _donatePanel.SetActive(false);
        }

        private void BuildConfirmQuitPanel(Transform parent)
        {
            _confirmQuitPanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(360, 200));
            _confirmQuitPanel.name = "ConfirmQuitPanel";

            CreateLabel(_confirmQuitPanel.transform, new Vector2(0, 50), "Are you sure?", 20);
            Button yesBtn = CreateButton(_confirmQuitPanel.transform, new Vector2(-70, -20), new Vector2(120, 32), "Yes");
            Button noBtn = CreateButton(_confirmQuitPanel.transform, new Vector2(70, -20), new Vector2(120, 32), "No");

            yesBtn.onClick.AddListener(() => Application.Quit());
            noBtn.onClick.AddListener(() => ShowConfirmQuit(false));

            _confirmQuitPanel.SetActive(false);
        }

        private void ShowConfirmQuit(bool show)
        {
            if (_confirmQuitPanel == null) return;
            _confirmQuitPanel.SetActive(show);
        }

        private void BuildInfoPanel(Transform parent)
        {
            _infoPanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(440, 220));
            _infoPanel.name = "InfoPanel";
            _infoText = CreateLabel(_infoPanel.transform, new Vector2(0, 30), "Info", 14);
            _infoText.alignment = TextAnchor.MiddleCenter;
            _infoText.rectTransform.sizeDelta = new Vector2(400, 120);

            Button okBtn = CreateButton(_infoPanel.transform, new Vector2(0, -70), new Vector2(140, 32), "OK");
            okBtn.onClick.AddListener(() => _infoPanel.SetActive(false));

            _infoPanel.SetActive(false);
        }

        private void ShowInfo(string message)
        {
            if (_infoPanel == null || _infoText == null) return;
            _infoText.text = message;
            _infoPanel.SetActive(true);
        }

        private void ShowDonate()
        {
            if (_donatePanel == null) return;
            _donatePanel.SetActive(true);
        }

        private void BuildAboutPanel(Transform parent)
        {
            _aboutPanel = CreatePanel(parent, new Vector2(0, 0), new Vector2(440, 260));
            _aboutPanel.name = "AboutPanel";

            CreateLabel(_aboutPanel.transform, new Vector2(0, 80), "About Me", 20);
            Text body = CreateLabel(_aboutPanel.transform, new Vector2(0, 20), "Developer: monte\nEmail: montetitan@gmail.com\nThanks for playing!", 12);
            body.alignment = TextAnchor.MiddleCenter;
            body.rectTransform.sizeDelta = new Vector2(400, 120);

            Button okBtn = CreateButton(_aboutPanel.transform, new Vector2(0, -80), new Vector2(140, 32), "OK");
            okBtn.onClick.AddListener(() => _aboutPanel.SetActive(false));

            _aboutPanel.SetActive(false);
        }

        private void ShowAbout()
        {
            if (_aboutPanel == null) return;
            _aboutPanel.SetActive(true);
        }

        private void ShowNewGamePanel()
        {
            if (_newGamePanel == null) return;
            Hide();
            _newGamePanel.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            if (_newGameRefreshRoutine != null)
            {
                StopCoroutine(_newGameRefreshRoutine);
            }
            _newGameRefreshRoutine = StartCoroutine(RefreshNewGameWhenReady());
        }

        private System.Collections.IEnumerator RefreshNewGameWhenReady()
        {
            float timer = 0f;
            while (timer < 3f)
            {
                GameManager gm = GameManager.Instance;
                if (gm != null && gm.Save != null && gm.MissionSystem != null)
                {
                    _selectedTwoWay = gm.Save.twoWayTraffic;
                    break;
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            RefreshNewGamePanel();
        }

        private void RefreshNewGamePanel()
        {
            if (_newGamePanel == null || !_newGamePanel.activeSelf) return;

            _modeEndlessBtn.GetComponentInChildren<Text>().text = "Endless (On)";
            _trafficTwoWayBtn.GetComponentInChildren<Text>().text = _selectedTwoWay ? "Two-Way (On)" : "Two-Way";
            _trafficOneWayBtn.GetComponentInChildren<Text>().text = _selectedTwoWay ? "One-Way" : "One-Way (On)";

            ClearChildren(_missionsListRoot);
            if (_missionsScrollRoot != null)
            {
                CanvasGroup scrollGroup = _missionsScrollRoot.GetComponent<CanvasGroup>();
                if (scrollGroup != null) scrollGroup.blocksRaycasts = false;
                Image scrollBg = _missionsScrollRoot.GetComponent<Image>();
                if (scrollBg != null) scrollBg.raycastTarget = false;
                _missionsScrollRoot.SetActive(false);
            }
        }

        private void OpenDonateUpi()
        {
            // UPI deep link for Google Pay (android will route to a UPI-capable app)
            string uri = "upi://pay?pa=montetitan@okicici&pn=TrafficRider%20Donate&cu=INR";
            Application.OpenURL(uri);
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

        private Image CreateDimOverlay(Transform parent)
        {
            GameObject overlayObj = new GameObject("MenuDimOverlay");
            overlayObj.transform.SetParent(parent);
            Image image = overlayObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.35f);
            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            overlayObj.SetActive(false);
            return image;
        }

        private Text CreateLabel(Transform parent, Vector2 anchoredPos, string text, int fontSize)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.color = new Color(0.9f, 0.9f, 0.9f);
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(300, 40);
            return label;
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

        private Button CreateListButton(Transform parent, string label)
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
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            return button;
        }

        private Text CreateListLabel(Transform parent, string text)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 12;
            label.color = new Color(0.9f, 0.9f, 0.9f);
            label.alignment = TextAnchor.MiddleCenter;
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360, 28);
            label.text = text;
            return label;
        }

        private Transform CreateScrollArea(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject root = new GameObject("ScrollRoot");
            root.transform.SetParent(parent);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPos;
            rootRect.sizeDelta = size;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.6f);

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

        private static void ClearChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
