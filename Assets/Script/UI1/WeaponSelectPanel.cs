using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
public class WeaponSelectPanel : MonoBehaviour
{
   private List<WeaponData> weapons;
   private Button selectedButton;
   private TMP_FontAsset font;
   private void Start()
   {

      RectTransform rect = GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(1f, 1f);
      rect.anchorMax = new Vector2(1f, 1f);
      rect.pivot = new Vector2(1f, 1f);
      rect.anchoredPosition = new Vector2(-20f, -20f);
      rect.sizeDelta = new Vector2(340f, 330f);

      font = Resources.Load<TMP_FontAsset>("Fonts/AlibabaPuHuiTi-3-55-Regular SDF");
      LoadData();
      BuildButtons();
   }

   private void LoadData()
   {
      TextAsset jsonFile = Resources.Load<TextAsset>("Data/weapon");
      if (jsonFile == null)
      {
         Debug.LogError("找不到武器数据: Resources/Data/weapon.json");
         return;
      }
      weapons = JsonConvert.DeserializeObject<List<WeaponData>>(jsonFile.text);
   }
   private void BuildButtons()
   {
      if (weapons == null || weapons.Count == 0) return;

      float topY = -10f;
      float btnWidth = 312f;
      float btnHeight = 62f;
      float gap = 8f;

      for (int i = 0; i < weapons.Count; i++)
      {
         WeaponData weap = weapons[i];
         Button btn = CreateButton(weap);

         RectTransform rect = (RectTransform)btn.transform;
         rect.anchorMin = new Vector2(0.5f, 1f);
         rect.anchorMax = new Vector2(0.5f, 1f);
         rect.pivot = new Vector2(0.5f, 1f);
         rect.anchoredPosition = new Vector2(0f, topY - i * (btnHeight + gap));
         rect.sizeDelta = new Vector2(btnWidth, btnHeight);
      }
   }
   private Button CreateButton(WeaponData weap)
   {
      GameObject go = CreateObject("Weapon_" + weap.name, transform);
      Image img = go.AddComponent<Image>();
      img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

      Button btn = go.AddComponent<Button>();
      btn.targetGraphic = img;

      string label = weap.name;
      GameObject textGo = CreateText(label, go.transform, 28);
      Stretch((RectTransform)textGo.transform);

      btn.onClick.AddListener(() => Select(weap, btn, img));
      return btn;
   }
   private void Select(WeaponData weap, Button btn, Image img)
   {
      GameData.selectedWeaponId = weap.id;

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
