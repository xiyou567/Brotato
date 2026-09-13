using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public static PropManager Instance;
    private List<PropData> props;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/prop");
        if (jsonFile == null)
        {
            Debug.LogError("找不到道具数据: Resources/Data/prop.json");
            return;
        }
        props = JsonConvert.DeserializeObject<List<PropData>>(jsonFile.text);
    }

    /// <summary>供商店面板读取道具列表</summary>
    public List<PropData> GetProps()
    {
        return props;
    }

    public void ApplyProp(int id)
    {
        if (props == null) return;

        PropData prop = props.Find(x => x.id == id);
        if (prop == null) return;

        Player player = Player.Instance;
        if (player == null) return;

        player.AddMaxHp(prop.maxHp);
        player.AddExpMultiplier(prop.expMuti);
        player.AddShopDiscount(prop.shopDiscount);
        player.AddPickRangePer(prop.pickRange);
        player.AddHpRegen(prop.revive);   // JSON 字段叫 revive，实际是「生命再生」

        // 短/长加成看当前这把武器属于哪边，近战吃 short_*，远程吃 long_*
        WeaponBase weapon = FindObjectOfType<WeaponBase>();
        if (weapon == null) return;

        bool isLong = weapon.IsLong;
        weapon.AddDamagePer(isLong ? prop.long_damage : prop.short_damage);
        weapon.AddAttackSpeedPer(isLong ? prop.long_attackSpeed : prop.short_attackSpeed);
        weapon.AddRangePer(isLong ? prop.long_range : prop.short_range);
        weapon.AddCritChance(prop.critical_strikes_probability);
    }








}
