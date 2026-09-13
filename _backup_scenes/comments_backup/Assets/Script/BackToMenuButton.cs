using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 结算界面的返回按钮，独立一整套。
/// 不挂在 ResultPanel（或场景里任何面板）下面，自己起一个 sortingOrder 很高的
/// ScreenSpaceOverlay Canvas，所以场景里的兄弟顺序、全屏透明 Image、raycast
/// 命中谁，都不会影响它。
/// </summary>
public class BackToMenuButton : MonoBehaviour
{
    private const string MenuSceneName = "01-MainMenu1";
    private const int OverlaySortingOrder = 30000;

    private static BackToMenuButton instance;

    private int shownFrame = -1;
    private bool loading;

    /// <summary>第一次调用时建出来，之后复用</summary>
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
        canvas.sortingOrder = OverlaySortingOrder;   // 压在所有游戏 UI 上面

        // 必须和场景里那个 Canvas 用同一套缩放参数，否则按钮和结算面板的
        // 相对位置会随分辨率漂移（两边各缩放各的）
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 全屏遮罩。alpha 给 0，看不见但仍然吃点击——层里已经有压暗的底图，
        // 这里再叠一层会把结算文字压得更黑。作用是别让点击漏到下面的游戏 UI。
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

        // 相对屏幕中心定位，x 跟结算面板中心对齐。
        // y 越小越靠下，改这一个数就行。
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
        text.raycastTarget = false;      // 文字挡住按钮就点不到了

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
        if (Time.frameCount == shownFrame) return;   // 弹出的那一帧不算

        // 按钮点不到时还有键盘
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            BackToMenu();
    }

    private void BackToMenu()
    {
        if (loading) return;   // onClick 和键盘可能同一帧都触发，防重复加载
        loading = true;

        Debug.Log("BackToMenuButton：返回主菜单");
        Time.timeScale = 1;    // 先恢复，否则主菜单也是冻结的

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
