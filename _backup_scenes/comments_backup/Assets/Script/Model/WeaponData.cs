using System;

[Serializable]
public class WeaponData
{
    public int id;
    public string name;
    public string avatar;
    public int grade;
    public int price;

    public float damage;
    public int isLong;          // 0=近战, 1=远程
    public float range;         // 攻击范围（像素单位）
    public float critical_strikes_multiple;
    public float critical_strikes_probability;
    public float cooling;       // 冷却时间（秒）
    public float repel;         // 击退距离
    public string describe;
}
