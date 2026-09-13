using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    private const float HoldSeconds = 1.4f;
    private const float FadeSeconds = 0.5f;

    private static Toast instance;

    private TextMeshProUGUI label;
    private CanvasGroup group;
    private float elapsed = -1f;

    public static void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

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

        toast.group = go.AddComponent<CanvasGroup>();
        toast.group.alpha = 0f;
        toast.group.blocksRaycasts = false;
        toast.group.interactable = false;

        return toast;
    }

    private void Pop(string message)
    {
        label.text = message;
        transform.SetAsLastSibling();
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
