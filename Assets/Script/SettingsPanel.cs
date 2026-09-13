using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public static SettingsPanel Instance;

    static Sprite whiteSprite;

    public static void Toggle()
    {
        if (Instance == null)
        {
            Build();
            return;
        }
        Instance.gameObject.SetActive(!Instance.gameObject.activeSelf);
    }

    static Sprite WhiteSprite()
    {
        if (whiteSprite == null)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, Color.white);
            t.Apply();
            whiteSprite = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        return whiteSprite;
    }

    static void Build()
    {
        Canvas canvas = FindRootCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("设置面板：找不到根 Canvas，不弹了");
            return;
        }

        GameObject root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        Instance = root.AddComponent<SettingsPanel>();

        RectTransform rr = (RectTransform)root.transform;
        Stretch(rr);
        Image dim = root.GetComponent<Image>();
        dim.sprite = WhiteSprite();
        dim.color = new Color(0f, 0f, 0f, 0.78f);
        dim.raycastTarget = true;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        Image pImg = panel.GetComponent<Image>();
        pImg.sprite = WhiteSprite();
        pImg.color = new Color(0.16f, 0.16f, 0.23f, 0.97f);
        RectTransform pr = (RectTransform)panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(480, 360);

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(panel.transform, false);
        RectTransform cr = (RectTransform)content.transform;
        Stretch(cr);
        cr.offsetMin = new Vector2(36, 26);
        cr.offsetMax = new Vector2(-36, -26);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        TextMeshProUGUI title = NewText(content.transform, "设  置", 30, Color.white, TextAlignmentOptions.Center);
        LayoutElement titleLE = title.gameObject.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 44;

        AudioManager am = AudioManager.Instance;
        float curMusic = am != null ? am.MusicVolume : 1f;
        float curSfx = am != null ? am.SfxVolume : 1f;
        BuildVolumeRow(content.transform, "背景音乐", curMusic, v =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
        });
        BuildVolumeRow(content.transform, "音效音量", curSfx, v =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
        });

        LayoutElement spacer = new GameObject("Spacer", typeof(RectTransform)).AddComponent<LayoutElement>();
        spacer.transform.SetParent(content.transform, false);
        spacer.flexibleHeight = 1;

        BuildCloseButton(content.transform);
    }

    static void BuildVolumeRow(Transform parent, string label, float initValue, UnityEngine.Events.UnityAction<float> onChanged)
    {
        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false;
        LayoutElement rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 56;

        TextMeshProUGUI nameText = NewText(row.transform, label, 24, new Color(0.9f, 0.9f, 0.95f), TextAlignmentOptions.Left);
        LayoutElement nameLE = nameText.gameObject.AddComponent<LayoutElement>();
        nameLE.preferredWidth = 130;
        nameLE.preferredHeight = 56;

        Slider slider = BuildSlider(row.transform);
        LayoutElement sliderLE = slider.gameObject.AddComponent<LayoutElement>();
        sliderLE.flexibleWidth = 1;
        sliderLE.preferredHeight = 56;

        TextMeshProUGUI pct = NewText(row.transform, "100%", 22, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right);
        LayoutElement pctLE = pct.gameObject.AddComponent<LayoutElement>();
        pctLE.preferredWidth = 62;
        pctLE.preferredHeight = 56;

        pct.text = Mathf.RoundToInt(initValue * 100f) + "%";
        slider.onValueChanged.AddListener(v =>
        {
            pct.text = Mathf.RoundToInt(v * 100f) + "%";
            onChanged(v);
        });
        slider.value = initValue;
    }

    static Slider BuildSlider(Transform parent)
    {
        GameObject rootGO = new GameObject("Slider", typeof(RectTransform));
        rootGO.transform.SetParent(parent, false);
        RectTransform rootRect = (RectTransform)rootGO.transform;

        Slider slider = rootGO.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.transition = Selectable.Transition.None;

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(rootRect, false);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.sprite = WhiteSprite();
        bgImg.color = new Color(0.28f, 0.28f, 0.36f, 1f);
        Stretch((RectTransform)bg.transform);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(rootRect, false);
        RectTransform fillAreaRect = (RectTransform)fillArea.transform;
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(2f, 0f);
        fillAreaRect.offsetMax = new Vector2(-2f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillAreaRect, false);
        Image fillImg = fill.GetComponent<Image>();
        fillImg.sprite = WhiteSprite();
        fillImg.color = new Color(1f, 0.85f, 0.3f, 1f);
        RectTransform fillRect = (RectTransform)fill.transform;
        Stretch(fillRect);
        slider.fillRect = fillRect;

        GameObject slideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        slideArea.transform.SetParent(rootRect, false);
        RectTransform slideAreaRect = (RectTransform)slideArea.transform;
        slideAreaRect.anchorMin = new Vector2(0f, 0.25f);
        slideAreaRect.anchorMax = new Vector2(1f, 0.75f);
        slideAreaRect.offsetMin = new Vector2(2f, 0f);
        slideAreaRect.offsetMax = new Vector2(-2f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(slideAreaRect, false);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.sprite = WhiteSprite();
        handleImg.color = Color.white;
        RectTransform handleRect = (RectTransform)handle.transform;
        handleRect.sizeDelta = new Vector2(22f, 22f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        return slider;
    }

    static void BuildCloseButton(Transform parent)
    {
        GameObject holder = new GameObject("CloseHolder", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        holder.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = holder.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;

        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        LayoutElement holderLE = holder.AddComponent<LayoutElement>();
        holderLE.preferredHeight = 50;

        GameObject btnGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(holder.transform, false);
        RectTransform btnRect = (RectTransform)btnGO.transform;
        btnRect.sizeDelta = new Vector2(150, 46);
        Image img = btnGO.GetComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = new Color(0.9f, 0.35f, 0.25f, 1f);
        Button btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;

        TextMeshProUGUI label = NewText(btnGO.transform, "关 闭", 24, Color.white, TextAlignmentOptions.Center);
        RectTransform lr = (RectTransform)label.transform;
        Stretch(lr);

        btn.onClick.AddListener(() =>
        {
            if (Instance != null) Instance.gameObject.SetActive(false);
        });
    }

    static TextMeshProUGUI NewText(Transform parent, string content, float fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (t.font == null)
            t.font = TMP_Settings.defaultFontAsset;
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Canvas FindRootCanvas()
    {
        Canvas[] all = FindObjectsOfType<Canvas>();
        foreach (Canvas c in all)
            if (c.isRootCanvas && c.enabled)
                return c;
        return null;
    }
}
