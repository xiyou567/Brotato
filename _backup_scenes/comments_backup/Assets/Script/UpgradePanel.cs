using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 升级面板：经验满级时弹出「三选一」属性卡，选择后生效并恢复游戏。
///
/// 按钮用「手动设置 RectTransform 坐标」而非 VerticalLayoutGroup ——
/// 运行时动态创建子物体时 LayoutGroup 不会立即重排，会导致按钮堆叠。
/// </summary>
public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel Instance;

    // 面板当前是否显示（Player 用它判断是否重复弹）
    public bool IsShowing { get; private set; }

    private Canvas canvas;
    private GameObject background;   // 全屏半透明背景，挡住点击
    private GameObject panel;

    private List<UpgradeOption> optionPool;

    private class UpgradeOption
    {
        public string label;
        public System.Action apply;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        BuildUI();
        Hide();
    }

    private void BuildUI()
    {
        canvas = FindRootCanvas();
        if (canvas == null)
        {
            Debug.LogError("UpgradePanel: 场景里找不到 Canvas！");
            return;
        }

        background = CreateObject("UpgradeBackground", canvas.transform);
        Image bg = background.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);
        Stretch((RectTransform)background.transform);

        panel = CreateObject("UpgradePanelBox", canvas.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.sizeDelta = new Vector2(520f, 420f);
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        // 标题占顶部往下 15~70px，按钮定位从 95 开始（见 Show）
        GameObject titleGo = CreateText("升级！选择一个属性", panel.transform, 30);
        RectTransform titleRect = (RectTransform)titleGo.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -15f);
        titleRect.sizeDelta = new Vector2(0f, 55f);

        optionPool = new List<UpgradeOption>
        {
            new UpgradeOption { label = "+5 最大生命", apply = () => Player.Instance.AddMaxHp(5f) },
            new UpgradeOption { label = "+0.5 移速",    apply = () => Player.Instance.AddSpeed(0.5f) },
            new UpgradeOption { label = "+3 攻击力",    apply = () => Player.Instance.AddAttack(3f) },
        };
    }

    /// <summary>要挂在根 Canvas 下，挂在子 Canvas 上会被它的裁剪/层级吃掉</summary>
    private static Canvas FindRootCanvas()
    {
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.isRootCanvas) return c;
        }
        return null;
    }

    public void Show()
    {
        if (panel == null) return;

        IsShowing = true;

        // 清掉上一次的选项按钮（只清按钮，不清标题/背景）
        foreach (Transform child in panel.transform)
        {
            if (child.name == "Option") Destroy(child.gameObject);
        }

        List<UpgradeOption> picked = PickRandom(optionPool, 3);

        // 手动定位：标题占 15~70，按钮从 95 开始，每个高 70 + 间距 20
        float btnWidth = 400f;
        float btnHeight = 70f;
        float gap = 20f;
        float topY = -95f;

        for (int i = 0; i < picked.Count; i++)
        {
            GameObject btnGo = CreateOptionButton(picked[i]);
            RectTransform rect = (RectTransform)btnGo.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topY - i * (btnHeight + gap));
            rect.sizeDelta = new Vector2(btnWidth, btnHeight);
        }

        background.SetActive(true);
        panel.SetActive(true);
    }

    private void Hide()
    {
        IsShowing = false;
        if (background != null) background.SetActive(false);
        if (panel != null) panel.SetActive(false);
    }

    private GameObject CreateOptionButton(UpgradeOption option)
    {
        GameObject btnGo = CreateObject("Option", panel.transform);
        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textGo = CreateText(option.label, btnGo.transform, 26);
        Stretch((RectTransform)textGo.transform);

        btn.onClick.AddListener(() =>
        {
            option.apply();
            Hide();
            Player.Instance.CompleteLevelUp();
        });

        return btnGo;
    }

    private List<UpgradeOption> PickRandom(List<UpgradeOption> pool, int count)
    {
        List<UpgradeOption> copy = new List<UpgradeOption>(pool);
        List<UpgradeOption> result = new List<UpgradeOption>();
        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }
        return result;
    }

    private GameObject CreateObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // 内置的 LegacyRuntime / Arial 都没有中文字形，中文会渲染成方块。
    // 全项目统一用这个 TMP 中文字体，懒加载一次存下来。
    private static TMP_FontAsset cnFont;

    private static TMP_FontAsset CnFont()
    {
        if (cnFont == null)
            cnFont = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
        return cnFont;
    }

    private GameObject CreateText(string content, Transform parent, int fontSize)
    {
        GameObject go = CreateObject("Text", parent);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        if (CnFont() != null) text.font = cnFont;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;   // 文字铺满整个按钮，别把点击吃掉
        return go;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
