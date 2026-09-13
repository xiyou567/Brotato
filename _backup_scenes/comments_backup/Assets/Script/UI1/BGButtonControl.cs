using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BGButtonControl : MonoBehaviour
{
    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClickStart);

        CreateLabel();
    }

    /// <summary>动态加「开始」文字，TMP 要配中文字体否则中文乱码</summary>
    private void CreateLabel()
    {
        if (transform.Find("Label") != null) return;

        GameObject go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(transform, false);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = "开始";
        text.font = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnClickStart()
    {
        if (string.IsNullOrEmpty(GameData.selectedLevelName))
        {
            Debug.Log("还没选难度！");
            Toast.Show("请先选难度（右下角）");
            return;
        }
        if (GameData.selectedRoleId == 0)
        {
            Debug.Log("还没选角色！");
            Toast.Show("请先选角色");
            return;
        }
        // 不选武器原来会静默用手枪开局，玩家根本不知道自己拿了什么
        if (GameData.selectedWeaponId == 0)
        {
            Debug.Log("还没选武器！");
            Toast.Show("请先选武器（右上角）");
            return;
        }
        SceneManager.LoadScene("03-GamePlay");
    }
}
