using UnityEngine;
using TMPro;

/// <summary>伤害飘字：挂在 HUD Canvas 下复用同一套 UGUI 文字，不用 3D TextMeshPro（项目字体不适配）</summary>
public class DamageNumber : MonoBehaviour
{
    public static void Show(Vector3 worldPos, float amount)
    {
        Canvas canvas = FindRootCanvas();
        if (canvas == null || Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransform canvasRect = (RectTransform)canvas.transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, CameraFor(canvas), out Vector2 localPos);

        // 必须带 RectTransform，否则挂不到 Canvas 下
        // 每次飘字都 new 一个 GameObject，高攻速下可考虑做对象池
        GameObject go = new GameObject("DamageNumber", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = localPos + new Vector2(Random.Range(-12f, 12f), 0f); // 防多颗重叠

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        if (text.font == null)
            text.font = TMP_Settings.defaultFontAsset;   // 项目默认字体，HUD 用的那个
        text.text = Mathf.RoundToInt(amount).ToString();
        text.fontSize = 28;
        text.color = Color.yellow;
        text.alignment = TextAlignmentOptions.Center;

        go.AddComponent<DamageNumber>();   // 挂自己：上飘 + 渐隐 + 销毁
    }

    /// <summary>HUD 都挂在根 Canvas 下</summary>
    static Canvas FindRootCanvas()
    {
        Canvas[] all = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in all)
            if (c.isRootCanvas && c.enabled)
                return c;
        return null;
    }

    /// <summary>Overlay 模式要传 null，Camera 模式要传它的世界相机</summary>
    static Camera CameraFor(Canvas c)
    {
        return c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
    }

    private float life = 0.6f;
    private float age = 0f;
    private RectTransform rt;
    private TextMeshProUGUI tmp;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        age += Time.deltaTime;

        rt.anchoredPosition += Vector2.up * (60f * Time.deltaTime);

        float alpha = 1f - age / life;
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, Mathf.Clamp01(alpha));

        if (age >= life)
            Destroy(gameObject);
    }
}
