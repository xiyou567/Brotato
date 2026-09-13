using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficuleySelectPanel : MonoBehaviour
{
    private List<DifficultyData> difficulties;
    private Button selectedButton;
    private TMP_FontAsset font;

    private void Start()
    {

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-20f, 20f);
        rect.sizeDelta = new Vector2(340f, 420f);

        font = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
        LoadData();
        BuildButtons();
    }

    private void LoadData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/difficulty");
        if (jsonFile == null)
        {
            Debug.LogError("找不到难度数据: Resources/Data/difficulty.json");
            return;
        }
        difficulties = JsonConvert.DeserializeObject<List<DifficultyData>>(jsonFile.text);
    }

    private void BuildButtons()
    {
        if (difficulties == null || difficulties.Count == 0) return;

        float topY = -10f;
        float btnWidth = 312f;
        float btnHeight = 66f;
        float gap = 8f;

        for (int i = 0; i < difficulties.Count; i++)
        {
            DifficultyData diff = difficulties[i];
            Button btn = CreateButton(diff);

            RectTransform rect = (RectTransform)btn.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topY - i * (btnHeight + gap));
            rect.sizeDelta = new Vector2(btnWidth, btnHeight);
        }
    }

    private Button CreateButton(DifficultyData diff)
    {
        GameObject go = CreateObject("Difficulty_" + diff.name, transform);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        string label = diff.name;
        if (!string.IsNullOrEmpty(diff.describe))
            label += "\n" + diff.describe;
        GameObject textGo = CreateText(label, go.transform, 24);
        Stretch((RectTransform)textGo.transform);

        btn.onClick.AddListener(() => Select(diff, btn, img));
        return btn;
    }

    private void Select(DifficultyData diff, Button btn, Image img)
    {
        GameData.selectedLevelName = diff.levelName;
        Debug.Log("选择难度: " + diff.name + " -> " + diff.levelName);

        if (selectedButton != null)
            selectedButton.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);

        img.color = new Color(0.3f, 0.7f, 0.3f, 1f);
        selectedButton = btn;
    }

    private GameObject CreateObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private GameObject CreateText(string content, Transform parent, int fontSize)
    {
        GameObject go = CreateObject("Text", parent);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
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
