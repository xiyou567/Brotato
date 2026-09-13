using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕提示条：玩家操作不合法时给一句看得见的提示。
/// 这些地方原来只有 Debug.Log，玩的人看不到，点一下没反应像是卡住了。
///
/// 不用挂到场景里：第一次调用时自己建到根 Canvas 下，之后复用。
/// 淡出走 unscaled 时间，所以商店把 timeScale 冻成 0 的时候照样显示。
/// </summary>
public class Toast : MonoBehaviour
{
    private const float HoldSeconds = 1.4f;   // 停留多久
    private const float FadeSeconds = 0.5f;   // 淡出多久

    private static Toast instance;

    private TextMeshProUGUI label;
    private CanvasGroup group;
    private float elapsed = -1f;              // < 0 表示当前没有提示在显示

    /// <summary>弹一句提示。连着调用会顶掉上一句并重新计时，不会叠一摞</summary>
    public static void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        // 切场景后旧实例会被销毁，Unity 重载过的 == null 能认出来，于是重建
        if (instance == null) instance = Build();
        if (instance == null) return;

        instance.Pop(message);
    }

    private static Toast Build()
    {
        Canvas canvas = null;
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.isRootCanvas)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            Debug.LogWarning("Toast：场景里找不到 Canvas，提示显示不出来");
            return null;
        }

        GameObject go = new GameObject("Toast", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        // 贴屏幕上边中间：商店面板和升级面板都占着屏幕中央，这里不会打架
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -60f);
        rect.sizeDelta = new Vector2(600f, 84f);

        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.8f);
        bg.raycastTarget = false;

        GameObject textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 0f);
        textRect.offsetMax = new Vector2(-12f, 0f);

        Toast toast = go.AddComponent<Toast>();
        toast.label = textGo.AddComponent<TextMeshProUGUI>();
        toast.label.text = "";
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
        if (font != null) toast.label.font = font;
        toast.label.fontSize = 30;
        toast.label.alignment = TextAlignmentOptions.Center;
        toast.label.color = Color.white;
        toast.label.raycastTarget = false;

        // 只是个提示条，两件事都得关掉：别吃点击、别拦射线
        toast.group = go.AddComponent<CanvasGroup>();
        toast.group.alpha = 0f;
        toast.group.blocksRaycasts = false;
        toast.group.interactable = false;

        return toast;
    }

    private void Pop(string message)
    {
        label.text = message;
        transform.SetAsLastSibling();   // 压在商店 / 升级面板上面
        group.alpha = 1f;
        elapsed = 0f;
    }

    private void Update()
    {
        if (elapsed < 0f) return;

        elapsed += Time.unscaledDeltaTime;

        float alpha = 1f;
        if (elapsed > HoldSeconds)
            alpha = 1f - (elapsed - HoldSeconds) / FadeSeconds;

        if (alpha <= 0f)
        {
            elapsed = -1f;
            group.alpha = 0f;
            return;
        }

        group.alpha = alpha;
    }
}
