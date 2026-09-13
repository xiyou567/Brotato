using UnityEngine;
using TMPro;

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

        GameObject go = new GameObject("DamageNumber", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = localPos + new Vector2(Random.Range(-12f, 12f), 0f);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        if (text.font == null)
            text.font = TMP_Settings.defaultFontAsset;
        text.text = Mathf.RoundToInt(amount).ToString();
        text.fontSize = 28;
        text.color = Color.yellow;
        text.alignment = TextAlignmentOptions.Center;

        go.AddComponent<DamageNumber>();
    }

    static Canvas FindRootCanvas()
    {
        Canvas[] all = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in all)
            if (c.isRootCanvas && c.enabled)
                return c;
        return null;
    }

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
