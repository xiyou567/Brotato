using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image image;
    private TextMeshProUGUI text;

    // 记下按钮本来的配色：不是每个按钮都是黑底白字，
    // 移开鼠标时写死颜色会把原来的配色冲掉
    private Color imageColor;
    private Color textColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        if (image != null) imageColor = image.color;
        if (text != null) textColor = text.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null) image.color = Color.white;
        if (text != null) text.color = Color.black;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null) image.color = imageColor;
        if (text != null) text.color = textColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlaySound("菜单音效");
    }
}