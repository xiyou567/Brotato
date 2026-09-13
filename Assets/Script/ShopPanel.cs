using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    public static ShopPanel Instance;

    public bool IsShowing { get; private set; }

    private Canvas canvas;
    private GameObject background;
    private GameObject panel;

    private List<PropData> props;
    private readonly HashSet<int> boughtIds = new HashSet<int>();

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
            Debug.LogError("ShopPanel: 场景里找不到 Canvas！");
            return;
        }

        background = CreateObject("ShopBackground", canvas.transform);
        Image bg = background.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);
        Stretch((RectTransform)background.transform);

        panel = CreateObject("ShopPanelBox", canvas.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.sizeDelta = new Vector2(570f, 532f);
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject titleGo = CreateText("商店", panel.transform, 32);
        RectTransform titleRect = (RectTransform)titleGo.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -15f);
        titleRect.sizeDelta = new Vector2(0f, 45f);

        GameObject btnGo = CreateObject("ContinueBtn", panel.transform);
        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.3f, 1f);
        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        GameObject btnText = CreateText("继续", btnGo.transform, 26);
        Stretch((RectTransform)btnText.transform);
        RectTransform btnRect = (RectTransform)btnGo.transform;
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0f, 20f);
        btnRect.sizeDelta = new Vector2(200f, 50f);
        btn.onClick.AddListener(Hide);
    }

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

        if (PropManager.Instance == null)
        {
            Debug.LogError("ShopPanel: 场景里没有 PropManager！");
            return;
        }

        IsShowing = true;
        props = PropManager.Instance.GetProps();

        foreach (Transform child in panel.transform)
        {
            if (child.name == "Item") Destroy(child.gameObject);
        }

        List<PropData> available = props.FindAll(x => !boughtIds.Contains(x.id));
        List<PropData> display = PickRandom(available, 4);

        float topY = -70f;
        float btnWidth = 480f;
        float btnHeight = 90f;
        float gap = 10f;

        for (int i = 0; i < display.Count; i++)
        {
            GameObject itemGo = CreateItemButton(display[i]);
            RectTransform rect = (RectTransform)itemGo.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topY - i * (btnHeight + gap));
            rect.sizeDelta = new Vector2(btnWidth, btnHeight);
        }

        background.SetActive(true);
        panel.SetActive(true);
    }

    public void Hide()
    {
        IsShowing = false;
        if (background != null) background.SetActive(false);
        if (panel != null) panel.SetActive(false);
    }

    private GameObject CreateItemButton(PropData prop)
    {
        GameObject itemGo = CreateObject("Item", panel.transform);
        Image img = itemGo.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button btn = itemGo.AddComponent<Button>();
        btn.targetGraphic = img;

        string label = prop.name + "    " + PriceOf(prop) + "金币\n" + prop.describe;
        GameObject textGo = CreateText(label, itemGo.transform, 18);
        Stretch((RectTransform)textGo.transform);

        btn.onClick.AddListener(() => Buy(prop));
        return itemGo;
    }

    private static int PriceOf(PropData prop)
    {
        float discount = Player.Instance != null ? Player.Instance.shopDiscount : 0f;
        return Mathf.Max(1, Mathf.RoundToInt(prop.price * (1f + discount)));
    }

    private void Buy(PropData prop)
    {
        Player player = Player.Instance;
        if (player == null) return;

        int price = PriceOf(prop);
        if (player.money < price)
        {
            Debug.Log("钱不够！需要 " + price + "，当前 " + player.money);
            Toast.Show("金币不够，还差 " + (price - player.money));
            return;
        }

        player.money -= price;
        GamePanel.Instance?.RenewMoney();

        PropManager.Instance.ApplyProp(prop.id);
        boughtIds.Add(prop.id);

        Show();
    }

    private List<PropData> PickRandom(List<PropData> pool, int count)
    {
        List<PropData> copy = new List<PropData>(pool);
        List<PropData> result = new List<PropData>();
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
        text.raycastTarget = false;
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
