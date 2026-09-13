using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToMenuButton : MonoBehaviour
{
    private const string MenuSceneName = "01-MainMenu1";
    private const int OverlaySortingOrder = 30000;

    private static BackToMenuButton instance;

    private int shownFrame = -1;
    private bool loading;

    public static BackToMenuButton Ensure()
    {
        if (instance != null) return instance;

        BackToMenuButton existing = FindObjectOfType<BackToMenuButton>();
        if (existing != null)
        {
            instance = existing;
            return instance;
        }

        instance = Build();
        return instance;
    }

    private static BackToMenuButton Build()
    {
        GameObject canvasGo = new GameObject("BackToMenuCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = OverlaySortingOrder;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject maskGo = new GameObject("Mask", typeof(RectTransform));
        maskGo.transform.SetParent(canvasGo.transform, false);
        Image mask = maskGo.AddComponent<Image>();
        mask.color = new Color(0f, 0f, 0f, 0f);
        Stretch((RectTransform)maskGo.transform);

        GameObject btnGo = new GameObject("BackToMenu", typeof(RectTransform));
        btnGo.transform.SetParent(canvasGo.transform, false);
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        RectTransform rect = (RectTransform)btnGo.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(21f, -230f);
        rect.sizeDelta = new Vector2(220f, 64f);

        GameObject textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = "返回主菜单";
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
        if (font != null) text.font = font;
        Stretch((RectTransform)textGo.transform);

        BackToMenuButton component = canvasGo.AddComponent<BackToMenuButton>();
        btn.onClick.AddListener(component.BackToMenu);

        canvasGo.SetActive(false);
        return component;
    }

    public void Show()
    {
        Debug.Log("BackToMenuButton：显示返回按钮");

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        shownFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount == shownFrame) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            BackToMenu();
    }

    private void BackToMenu()
    {
        if (loading) return;
        loading = true;

        Debug.Log("BackToMenuButton：返回主菜单");
        Time.timeScale = 1;

        try
        {
            SceneManager.LoadScene(MenuSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("按名字加载 " + MenuSceneName + " 失败：" + e.Message + "，改用索引 1");
            SceneManager.LoadScene(1);
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
