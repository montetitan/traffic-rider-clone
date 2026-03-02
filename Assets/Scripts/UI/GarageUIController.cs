using TrafficRider.Core;
using TrafficRider.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TrafficRider.UI
{
    public class GarageUIController : MonoBehaviour
    {
        private GameObject _panel;
        private Text _coins;
        private Transform _bikeList;
        private Transform _upgradeList;
        private RawImage _previewImage;
        private GaragePreview _preview;

        public void Build(Transform parent)
        {
            _panel = CreatePanel(parent, new Vector2(-20, -80), new Vector2(420, 520));
            _panel.name = "GaragePanel";

            _coins = CreateLabel(_panel.transform, new Vector2(16, -16), "Coins: 0", 18, TextAnchor.UpperLeft);

            CreateLabel(_panel.transform, new Vector2(16, -52), "Bikes", 18, TextAnchor.UpperLeft);
            _bikeList = CreateScrollArea(_panel.transform, new Vector2(16, -78), new Vector2(388, 150)).transform;

            _previewImage = CreatePreviewArea(_panel.transform, new Vector2(16, -238), new Vector2(388, 120));
            _preview = gameObject.AddComponent<GaragePreview>();
            _preview.Build(_previewImage, new Vector2(388, 120));

            CreateLabel(_panel.transform, new Vector2(16, -370), "Upgrades", 18, TextAnchor.UpperLeft);
            _upgradeList = CreateScrollArea(_panel.transform, new Vector2(16, -396), new Vector2(388, 100)).transform;

            Hide();
        }

        public void Toggle()
        {
            if (_panel == null) return;
            bool show = !_panel.activeSelf;
            _panel.SetActive(show);
            if (show)
            {
                Refresh();
            }
        }

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (_panel == null) return;
            _panel.SetActive(false);
        }

        public void Refresh()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Config == null) return;

            _coins.text = "Coins: " + gm.Save.coins;

            ClearChildren(_bikeList);
            ClearChildren(_upgradeList);

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
                    _preview.ShowBike(localBike);
                    Refresh();
                });
            }

            foreach (UpgradeConfig upgrade in gm.Config.upgrades)
            {
                UpgradeConfig localUpgrade = upgrade;
                int level = gm.UpgradeSystem.GetLevel(localUpgrade.id);
                string label = localUpgrade.name + " L" + level + "/" + localUpgrade.maxLevel + " | $" + localUpgrade.basePrice * (level + 1);
                Button btn = CreateButton(_upgradeList, label);
                btn.onClick.AddListener(() =>
                {
                    if (gm.UpgradeSystem.TryBuyUpgrade(localUpgrade.id))
                    {
                        gm.RefreshBikeStats();
                    }
                    Refresh();
                });
            }

            BikeConfig selected = gm.BikeSelectionSystem.GetSelectedBike();
            if (selected != null)
            {
                _preview.ShowBike(selected);
            }
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        private GameObject CreatePanel(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(parent);
            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
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
            label.color = Color.white;
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

        private RawImage CreatePreviewArea(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject root = new GameObject("Preview");
            root.transform.SetParent(parent);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            GameObject rawObj = new GameObject("PreviewImage");
            rawObj.transform.SetParent(root.transform);
            RawImage raw = rawObj.AddComponent<RawImage>();

            RectTransform rawRect = rawObj.GetComponent<RectTransform>();
            rawRect.anchorMin = new Vector2(0, 0);
            rawRect.anchorMax = new Vector2(1, 1);
            rawRect.offsetMin = new Vector2(6, 6);
            rawRect.offsetMax = new Vector2(-6, -6);

            return raw;
        }

        private Button CreateButton(Transform parent, string text)
        {
            GameObject buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            Button button = buttonObj.AddComponent<Button>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360, 32);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 16;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            label.text = text;

            RectTransform textRect = label.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            return button;
        }
    }
}
