using System.Text;
using TrafficRider.Core;
using TrafficRider.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class UIController : MonoBehaviour
    {
        private static UIController _instance;
        private Text _speed;
        private Text _distance;
        private Text _mode;
        private Text _overtakes;
        private Text _missions;
        private GarageUIController _garage;
        private SettingsUIController _settings;
        private GameObject _gameOverPanel;
        private Text _gameOverStats;
        private Text _gameOverTitle;
        private GameObject _missionCompletePanel;
        private Text _missionCompleteStats;
        private Text _runCoins;
        private Text _trafficDebug;
        private AudioSource _hornSource;
        private Button _settingsBtn;
        private Button _homeBtn;
        private Button _quitBtn;
        private Button _wheelieBtn;
        private Button _pauseBtn;
        private GameObject _pausePanel;
        private Button _resumeBtn;
        private GameObject _confirmQuitPanel;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (_instance.gameObject != null)
                {
                    Destroy(_instance.gameObject);
                }
            }
            _instance = this;
            CreateUI();
        }

        private void Start()
        {
            if (_speed == null)
            {
                CreateUI();
            }
        }

        private void CreateUI()
        {
            if (_speed != null) return;
            if (this == null || gameObject == null) return;

            EnsureEventSystem();

            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            _speed = CreateLabel(canvas.transform, new Vector2(20, -20), "Speed");
            _distance = CreateLabel(canvas.transform, new Vector2(20, -60), "Distance");
            _mode = CreateLabel(canvas.transform, new Vector2(20, -100), "Mode");
            _overtakes = CreateLabel(canvas.transform, new Vector2(20, -140), "Overtakes");
            _missions = CreateLabel(canvas.transform, new Vector2(20, -180), "Missions");
            _runCoins = CreateLabel(canvas.transform, new Vector2(20, -220), "Run Coins");
            _trafficDebug = CreateSmallLabel(canvas.transform, new Vector2(20, -260), "Traffic");

            CreatePauseButton(canvas.transform);
            CreatePausePanel(canvas.transform);
            CreateConfirmQuitPanel(canvas.transform);
            CreateWheelieButton(canvas.transform);
            CreateHornButton(canvas.transform);

            _garage = gameObject.AddComponent<GarageUIController>();
            _garage.Build(canvas.transform);

            _settings = gameObject.AddComponent<SettingsUIController>();
            _settings.Build(canvas.transform);
            _settings.SetHomeAction(() =>
            {
                _settings.Hide();
                SetPaused(true);
            });
            _settings.SetShowQualitySection(false);


            CreateGameOverPanel(canvas.transform);
            CreateMissionCompletePanel(canvas.transform);
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void CreatePausePanel(Transform parent)
        {
            _pausePanel = new GameObject("PausePanel");
            _pausePanel.transform.SetParent(parent);
            Image bg = _pausePanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);
            RectTransform rect = _pausePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240, 240);
            rect.anchoredPosition = Vector2.zero;

            _resumeBtn = CreateButtonCentered(_pausePanel.transform, new Vector2(0, 60), new Vector2(160, 32), "Resume");
            _settingsBtn = CreateButtonCentered(_pausePanel.transform, new Vector2(0, 20), new Vector2(160, 32), "Settings");
            _homeBtn = CreateButtonCentered(_pausePanel.transform, new Vector2(0, -20), new Vector2(160, 32), "Home");
            _quitBtn = CreateButtonCentered(_pausePanel.transform, new Vector2(0, -60), new Vector2(160, 32), "Quit");

            _resumeBtn.onClick.AddListener(() => SetPaused(false));
            _settingsBtn.onClick.AddListener(() =>
            {
                if (_settings == null) return;
                _settings.Show();
            });
            _homeBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
            _quitBtn.onClick.AddListener(() => ShowConfirmQuit(true));

            _pausePanel.SetActive(false);
        }

        private void CreateConfirmQuitPanel(Transform parent)
        {
            _confirmQuitPanel = new GameObject("ConfirmQuitPanel");
            _confirmQuitPanel.transform.SetParent(parent);
            Image bg = _confirmQuitPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);
            bg.raycastTarget = false;

            RectTransform rect = _confirmQuitPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320, 180);
            rect.anchoredPosition = Vector2.zero;

            Text title = CreateCenteredLabel(_confirmQuitPanel.transform, new Vector2(0, 40), "Are you sure?", 20);
            title.color = Color.white;

            Button yesBtn = CreateButtonCentered(_confirmQuitPanel.transform, new Vector2(0, 0), new Vector2(140, 32), "Yes");
            yesBtn.onClick.AddListener(() => Application.Quit());

            Button noBtn = CreateButtonCentered(_confirmQuitPanel.transform, new Vector2(0, -40), new Vector2(140, 32), "No");
            noBtn.onClick.AddListener(() => ShowConfirmQuit(false));

            _confirmQuitPanel.SetActive(false);
        }

        private void CreatePauseButton(Transform parent)
        {
            _pauseBtn = CreateButton(parent, new Vector2(-20, -20), new Vector2(120, 32), "Menu");
            _pauseBtn.onClick.AddListener(() => SetPaused(true));
        }

        private void CreateHornButton(Transform parent)
        {
            _hornSource = gameObject.AddComponent<AudioSource>();
            _hornSource.loop = true;
            _hornSource.playOnAwake = false;
            _hornSource.spatialBlend = 0f;
            _hornSource.volume = 0.4f;
            _hornSource.clip = BuildHornClip();

            Button hornBtn = CreateButton(parent, new Vector2(-20, 20), new Vector2(120, 36), "Horn");
            RectTransform rect = hornBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-20, 20);

            HornButton hb = hornBtn.gameObject.AddComponent<HornButton>();
            hb.source = _hornSource;
        }

        private void CreateWheelieButton(Transform parent)
        {
            _wheelieBtn = CreateButton(parent, new Vector2(-20, 70), new Vector2(120, 36), "Wheelie");
            RectTransform rect = _wheelieBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-20, 70);

            WheelieButton wb = _wheelieBtn.gameObject.AddComponent<WheelieButton>();
            wb.cooldownSeconds = 5f;
            wb.boostMultiplier = 1.1f;
            wb.boostDuration = 2f;
        }

        private void CreateGameOverPanel(Transform parent)
        {
            _gameOverPanel = new GameObject("GameOverPanel");
            _gameOverPanel.transform.SetParent(parent);

            Image bg = _gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);
            bg.raycastTarget = false;

            RectTransform rect = _gameOverPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320, 200);
            rect.anchoredPosition = Vector2.zero;

            Text title = CreateCenteredLabel(_gameOverPanel.transform, new Vector2(0, 50), "Game Over", 26);
            title.color = Color.white;
            _gameOverTitle = title;

            _gameOverStats = CreateCenteredLabel(_gameOverPanel.transform, new Vector2(0, 10), "Stats", 16);
            _gameOverStats.color = Color.white;

            Button restartBtn = CreateButtonCentered(_gameOverPanel.transform, new Vector2(0, -40), new Vector2(160, 40), "Restart");
            restartBtn.onClick.AddListener(() => GameManager.Instance?.Restart());

            _gameOverPanel.SetActive(false);
        }

        private void CreateMissionCompletePanel(Transform parent)
        {
            _missionCompletePanel = new GameObject("MissionCompletePanel");
            _missionCompletePanel.transform.SetParent(parent);

            Image bg = _missionCompletePanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);
            bg.raycastTarget = false;

            RectTransform rect = _missionCompletePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(340, 220);
            rect.anchoredPosition = Vector2.zero;

            Text title = CreateCenteredLabel(_missionCompletePanel.transform, new Vector2(0, 60), "Mission Complete", 24);
            title.color = Color.white;

            _missionCompleteStats = CreateCenteredLabel(_missionCompletePanel.transform, new Vector2(0, 20), "Stats", 16);
            _missionCompleteStats.color = Color.white;

            Button restartBtn = CreateButtonCentered(_missionCompletePanel.transform, new Vector2(0, -30), new Vector2(160, 36), "Restart");
            restartBtn.onClick.AddListener(() => GameManager.Instance?.Restart());

            Button backBtn = CreateButtonCentered(_missionCompletePanel.transform, new Vector2(0, -70), new Vector2(160, 32), "Back");
            backBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));

            _missionCompletePanel.SetActive(false);
        }

        private Text CreateLabel(Transform parent, Vector2 anchoredPos, string label)
        {
            GameObject textObj = new GameObject(label);
            textObj.transform.SetParent(parent);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(500, 30);
            text.text = label + ": 0";
            return text;
        }

        private Text CreateSmallLabel(Transform parent, Vector2 anchoredPos, string label)
        {
            GameObject textObj = new GameObject(label);
            textObj.transform.SetParent(parent);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(600, 80);
            text.text = label;
            return text;
        }

        private Button CreateButton(Transform parent, Vector2 anchoredPos, Vector2 size, string label)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private Button CreateButtonCentered(Transform parent, Vector2 anchoredPos, Vector2 size, string label)
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
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private Text CreateCenteredLabel(Transform parent, Vector2 anchoredPos, string text, int size)
        {
            if (parent == null) return null;
            GameObject textObj = new GameObject("CenteredLabel");
            textObj.transform.SetParent(parent);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            label.raycastTarget = false;

            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(260, 40);
            return label;
        }

        private AudioClip BuildHornClip()
        {
            int sampleRate = 44100;
            float length = 1.0f;
            int sampleCount = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[sampleCount];
            float freq = 440f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
                float wave2 = Mathf.Sin(2f * Mathf.PI * (freq * 1.01f) * t) * 0.4f;
                data[i] = (wave + wave2) * 0.5f;
            }

            AudioClip clip = AudioClip.Create("Horn", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void SetSpeed(float kph)
        {
            if (_speed == null)
            {
                TryEnsureUI();
                if (_speed == null) return;
            }
            int shown = Mathf.Max(0, Mathf.RoundToInt(kph));
            _speed.text = "Speed: " + shown + " km/h";
        }

        public void SetDistance(float km)
        {
            if (_distance == null)
            {
                TryEnsureUI();
                if (_distance == null) return;
            }
            _distance.text = "Distance: " + km.ToString("F2") + " km";
        }

        public void SetCoins(int coins)
        {
            // gameplay HUD uses Run Coins only
        }

        public void SetMode(string mode)
        {
            if (_mode == null)
            {
                TryEnsureUI();
                if (_mode == null) return;
            }
            _mode.text = "Mode: " + mode;
        }

        public void SetOvertakes(int count)
        {
            if (_overtakes == null)
            {
                TryEnsureUI();
                if (_overtakes == null) return;
            }
            _overtakes.text = "Overtakes: " + count;
        }

        public void SetMissions(MissionSystem missionSystem)
        {
            if (_missions == null)
            {
                return;
            }
            if (missionSystem == null)
            {
                _missions.text = "Missions: (Disabled)";
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("Missions: ");
            foreach (var progress in missionSystem.GetProgress())
            {
                var cfg = missionSystem.GetConfig(progress.id);
                if (cfg == null) continue;
                sb.Append(cfg.name);
                sb.Append(" ");
                sb.Append(progress.value);
                sb.Append("/");
                sb.Append(cfg.target);
                if (progress.completed) sb.Append(" (Done)");
                sb.Append(" | ");
            }
            _missions.text = sb.ToString();
        }

        public void SetRunCoins(int coins)
        {
            if (_runCoins != null)
            {
                _runCoins.text = "Run Coins: " + coins;
            }
        }

        public void SetTrafficDebug(string text)
        {
            if (_trafficDebug == null)
            {
                TryEnsureUI();
                if (_trafficDebug == null) return;
            }
            _trafficDebug.text = text;
        }

        public void SetTopScore(float km, string userId)
        {
            if (_gameOverStats != null && _gameOverPanel != null && _gameOverPanel.activeSelf)
            {
                // Leave game over stats as-is; top score is shown in settings
            }
            if (_settings != null)
            {
                _settings.SetTopScore(km, userId);
            }
        }

        private void TryEnsureUI()
        {
            if (_speed != null) return;
            CreateUI();
        }

        public void ShowGameOver(bool show, float distanceMeters = 0f, int overtakes = 0, int runCoins = 0)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(show);
                if (show && _gameOverStats != null)
                {
                    float km = distanceMeters / 1000f;
                    _gameOverStats.text = "Distance " + km.ToString("F2") + " km | Overtakes " + overtakes + " | Coins " + runCoins;
                }
                if (show && _gameOverTitle != null && string.IsNullOrEmpty(_gameOverTitle.text))
                {
                    _gameOverTitle.text = "Game Over";
                }
            }

            if (_settings != null)
            {
                _settings.ShowBikeSection(show);
            }
        }

        public void SetPaused(bool paused)
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(paused);
            }
            ShowConfirmQuit(false);
            if (paused)
            {
                GameManager.Instance?.Pause(true);
                Time.timeScale = 0f;
            }
            else
            {
                if (_settings != null) _settings.Hide();
                GameManager.Instance?.Pause(false);
                Time.timeScale = 1f;
            }
        }

        private void ShowConfirmQuit(bool show)
        {
            if (_confirmQuitPanel != null)
            {
                _confirmQuitPanel.SetActive(show);
            }
        }

        public void ShowMissionComplete(bool show, float distanceMeters = 0f, int overtakes = 0, int runCoins = 0)
        {
            if (_missionCompletePanel != null)
            {
                _missionCompletePanel.SetActive(show);
                if (show && _missionCompleteStats != null)
                {
                    float km = distanceMeters / 1000f;
                    _missionCompleteStats.text = "Distance " + km.ToString("F2") + " km | Overtakes " + overtakes + " | Coins " + runCoins;
                }
            }
        }

        public void SetGameOverTitle(string title)
        {
            if (_gameOverTitle != null)
            {
                _gameOverTitle.text = title;
            }
        }
    }
}
