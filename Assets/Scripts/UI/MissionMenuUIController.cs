using System;
using TrafficRider.Core;
using TrafficRider.Data;
using TrafficRider.Systems;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

namespace TrafficRider.UI
{
    public class MissionMenuUIController : MonoBehaviour
    {
        private GameObject _panel;
        private Transform _missionList;
        private Text _coins;
        private Button _backBtn;
        private Action _backAction;
        private Coroutine _refreshRoutine;
        private GameConfig _configLocal;
        private SaveData _saveLocal;
        private MissionSystem _missionSystemLocal;
        private Coroutine _ensureRoutine;

        public void Build(Transform parent)
        {
            _panel = CreatePanel(parent, new Vector2(0, 0), new Vector2(420, 520));
            _panel.name = "MissionMenu";

            _coins = CreateLabelTopLeft(_panel.transform, new Vector2(16, -16), "Coins: 0", 18);
            CreateLabelTopLeft(_panel.transform, new Vector2(16, -44), "Missions", 20);

            _missionList = CreateScrollArea(_panel.transform, new Vector2(16, -90), new Vector2(388, 340)).transform;

            _backBtn = CreateButtonTopLeft(_panel.transform, new Vector2(280, -456), new Vector2(120, 32), "Back");
            _backBtn.onClick.AddListener(GoBack);

            Hide();
            StartEnsureLoad();
        }

        public void SetBackAction(Action action)
        {
            _backAction = action;
        }

        // Backward compatibility for existing callers.
        public void SetHomeAction(Action action)
        {
            SetBackAction(action);
        }

        public void Show()
        {
            EnsureBuilt();
            if (_panel == null) return;
            _panel.SetActive(true);
            if (_backBtn != null)
            {
                _backBtn.transform.SetAsLastSibling();
                _backBtn.interactable = true;
                Image backImg = _backBtn.GetComponent<Image>();
                if (backImg != null) backImg.raycastTarget = true;
            }
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
            }
            _refreshRoutine = StartCoroutine(RefreshWhenReady());
            StartEnsureLoad();
        }

        private void EnsureBuilt()
        {
            if (_panel != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            if (canvas == null)
            {
                GameObject uiObj = new GameObject("MissionMenuUI");
                canvas = uiObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiObj.AddComponent<CanvasScaler>();
                uiObj.AddComponent<GraphicRaycaster>();
            }

            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            Build(canvas.transform);
        }

        private void Update()
        {
            if (_panel != null && _panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                GoBack();
                return;
            }

            if (_panel == null || !_panel.activeSelf || _backBtn == null) return;

            if (Input.GetMouseButtonUp(0) && IsPointerInsideBack(Input.mousePosition))
            {
                GoBack();
                return;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended && IsPointerInsideBack(touch.position))
                {
                    GoBack();
                }
            }
        }

        private void GoBack()
        {
            if (_backAction != null)
            {
                _backAction();
                return;
            }

            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                return;
            }

            if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings > 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        private bool IsPointerInsideBack(Vector2 screenPos)
        {
            if (_backBtn == null) return false;
            RectTransform rect = _backBtn.GetComponent<RectTransform>();
            if (rect == null) return false;
            Canvas canvas = _backBtn.GetComponentInParent<Canvas>();
            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, cam);
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
            // wait one frame to let GameManager initialize if present
            yield return null;
            if (_missionList != null && _missionList.childCount > 0)
            {
                _ensureRoutine = null;
                yield break;
            }
            yield return LoadLocalConfig();
            RefreshLocal();
            _ensureRoutine = null;
        }

        private System.Collections.IEnumerator RefreshWhenReady()
        {
            float timer = 0f;
            while (timer < 3f)
            {
                GameManager gm = GameManager.Instance;
                if (gm == null)
                {
                    GameObject gmObj = new GameObject("GameManager");
                    gm = gmObj.AddComponent<GameManager>();
                    gm.InitializeMenuOnly();
                }
                if (gm != null && gm.Save != null && gm.MissionSystem != null)
                {
                    break;
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            GameManager ready = GameManager.Instance;
            if (ready != null && ready.Save != null && ready.MissionSystem != null)
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
            if (gm == null || gm.Save == null || gm.MissionSystem == null)
            {
                if (_missionSystemLocal != null && _saveLocal != null)
                {
                    RefreshLocal();
                }
                else
                {
                    StartEnsureLoad();
                }
                return;
            }

            _coins.text = "Coins: " + gm.Save.coins;

            ClearChildren(_missionList);
            if (gm.MissionSystem.GetProgress() == null)
            {
                CreateLabel(_missionList, Vector2.zero, "No missions found.", 14, TextAnchor.MiddleCenter);
                Debug.LogWarning("MissionMenu: no missions in config");
                return;
            }
            foreach (var progress in gm.MissionSystem.GetProgress())
            {
                var cfg = gm.MissionSystem.GetConfig(progress.id);
                if (cfg == null) continue;
                string mark = progress.completed ? "[x] " : "[ ] ";
                string label = mark + cfg.name + " (" + progress.value + "/" + cfg.target + ")";

                Button btn = CreateButton(_missionList, label);
                btn.onClick.AddListener(() =>
                {
                    gm.Save.selectedMissionId = cfg.id;
                    SaveSystem.Save(gm.Save);
                    Refresh();
                });
            }

            if (_missionList.childCount == 0)
            {
                CreateLabel(_missionList, Vector2.zero, "No missions found.", 14, TextAnchor.MiddleCenter);
                StartEnsureLoad();
            }
        }

        private System.Collections.IEnumerator LoadLocalConfig()
        {
            if (_configLocal != null && _missionSystemLocal != null) yield break;
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
            _missionSystemLocal = new MissionSystem(_configLocal, _saveLocal);
        }

        private void LoadLocalConfigSync()
        {
            if (_configLocal != null && _missionSystemLocal != null) return;
            string path = Path.Combine(Application.streamingAssetsPath, ConfigLoader.ConfigFileName);
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            _configLocal = JsonUtility.FromJson<GameConfig>(json);
            if (_configLocal == null) return;
            _saveLocal = SaveSystem.Load(_configLocal);
            _missionSystemLocal = new MissionSystem(_configLocal, _saveLocal);
        }

        private void RefreshLocal()
        {
            if (_saveLocal == null || _missionSystemLocal == null) return;
            _coins.text = "Coins: " + _saveLocal.coins;
            ClearChildren(_missionList);
            foreach (var progress in _missionSystemLocal.GetProgress())
            {
                var cfg = _missionSystemLocal.GetConfig(progress.id);
                if (cfg == null) continue;
                string mark = progress.completed ? "[x] " : "[ ] ";
                string label = mark + cfg.name + " (" + progress.value + "/" + cfg.target + ")";

                Button btn = CreateButton(_missionList, label);
                btn.onClick.AddListener(() =>
                {
                    _saveLocal.selectedMissionId = cfg.id;
                    SaveSystem.Save(_saveLocal);
                    RefreshLocal();
                });
            }

            if (_missionList.childCount == 0)
            {
                CreateLabel(_missionList, Vector2.zero, "No missions found.", 14, TextAnchor.MiddleCenter);
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
    }
}
