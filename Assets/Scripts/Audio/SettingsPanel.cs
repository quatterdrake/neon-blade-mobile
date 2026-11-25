using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Builds the volume panel in code: same panel for Settings (main menu) and Pause (game).
// Three sliders: Master, Music, SFX. Values are saved when the panel closes.
public static class SettingsPanel
{
    public struct Btn
    {
        public string label; public Action action;
        public Btn(string label, Action action) { this.label = label; this.action = action; }
    }

    static GameObject root;
    static readonly Color PanelColor = new Color32(20, 12, 38, 245);
    static readonly Color Pink = new Color32(255, 42, 109, 255);
    static readonly Color Cyan = new Color32(5, 217, 232, 255);

    public static bool IsOpen => root != null;

    public static void Show(string title, params Btn[] buttons)
    {
        Hide(false);

        root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.layer = 5; // UI
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;                      // above the game HUD
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // dark full-screen layer: blocks taps on the game behind the panel
        RectTransform dim = NewUI("Dim", root.transform);
        Stretch(dim);
        dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform panel = NewUI("Panel", dim);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(1000f, 0f);
        panel.gameObject.AddComponent<Image>().color = PanelColor;
        var v = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(60, 60, 40, 50);
        v.spacing = 24f;
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Label(panel, title, 72f, Pink, TextAlignmentOptions.Center, 100f);
        SliderRow(panel, "Master", VolumeKind.Master);
        SliderRow(panel, "Music", VolumeKind.Music);
        SliderRow(panel, "SFX", VolumeKind.SFX);
        foreach (var b in buttons) MakeButton(panel, b.label, b.action);
    }

    public static void Hide(bool save = true)
    {
        if (root != null) UnityEngine.Object.Destroy(root);
        root = null;
        if (save) SaveManager.Save();   // the settings test: close the app, open it, same volume
    }

    // ------------------------------------------------------------ building blocks

    static void SliderRow(Transform parent, string name, VolumeKind kind)
    {
        RectTransform row = NewUI(name + "Row", parent);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 30f;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = false;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 90f;

        Label(row, name, 48f, Color.white, TextAlignmentOptions.MidlineLeft, 90f, 220f);

        GameObject sGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sGo.name = name + "Slider";
        sGo.transform.SetParent(row, false);
        var le = sGo.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f; le.minWidth = 300f; le.preferredHeight = 70f;

        var slider = sGo.GetComponent<Slider>();
        slider.minValue = VolumeSettings.Min;
        slider.maxValue = 1f;

        sGo.transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        sGo.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Pink;
        Transform handle = sGo.transform.Find("Handle Slide Area/Handle");
        handle.GetComponent<Image>().color = Cyan;
        ((RectTransform)handle).sizeDelta = new Vector2(50f, 0f);   // big enough for a finger

        TextMeshProUGUI value = Label(row, "", 44f, Cyan, TextAlignmentOptions.MidlineRight, 90f, 140f);

        slider.value = VolumeSettings.Get(kind);       // set before the listener: no extra write
        value.text = Percent(slider.value);
        slider.onValueChanged.AddListener(x =>
        {
            VolumeSettings.Set(kind, x);
            value.text = Percent(x);
        });
    }

    static void MakeButton(Transform parent, string text, Action onClick)
    {
        RectTransform rt = NewUI(text + "Button", parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = Pink;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => { AudioManager.Click(); onClick?.Invoke(); });
        rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;

        TextMeshProUGUI label = Label(rt, text, 52f, Color.white, TextAlignmentOptions.Center, 110f);
        Stretch((RectTransform)label.transform);
    }

    static TextMeshProUGUI Label(Transform parent, string text, float size, Color color,
                                 TextAlignmentOptions align, float height, float width = -1f)
    {
        RectTransform rt = NewUI("Text", parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        if (width > 0f) le.preferredWidth = width;
        return t;
    }

    static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";
}
