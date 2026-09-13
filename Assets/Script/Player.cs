using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Player : MonoBehaviour
{
    public static Player Instance;
    private Color originColor;
    [SerializeField] private float speed = 4f;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float hp = 15f;
    public float maxHp = 15f;
    public int money = 30;
    public float exp = 0;
    public int killCount;
    public bool isDead;

    public int level = 1;
    public int needExp = 12;

    [Header("地图边界（空气墙）")]
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float mapMargin = 0.6f;

    public float expMultiplier = 1f;
    public float shopDiscount;
    public float pickRange = 1f;
    public float hpRegen;

    private int pendingLevelUps;

    private SpriteRenderer sr;

    private void Awake()
    {
        Instance = this;
        sr = spriteRenderer != null ? spriteRenderer : GetComponentInChildren<SpriteRenderer>();
        originColor = sr.color;
    }

    private Bounds mapBounds;
    private bool hasMapBounds;

    private void Start()
    {
        SpawnWeapon();
        ApplyRolePassive();
        CacheMapBounds();
    }

    private void CacheMapBounds()
    {
        if (mapRoot == null)
        {
            GameObject map = GameObject.Find("Map");
            if (map != null) mapRoot = map.transform;
        }

        if (mapRoot == null)
        {
            Debug.LogWarning("Player：场景里找不到 Map，空气墙不生效");
            return;
        }

        SpriteRenderer mapSr = mapRoot.GetComponent<SpriteRenderer>();
        if (mapSr == null)
        {
            Debug.LogWarning("Player：Map 上没有 SpriteRenderer，空气墙不生效");
            return;
        }

        Bounds b = mapSr.bounds;
        if (b.size.x < 0.01f || b.size.y < 0.01f)
        {

            Debug.LogWarning("Player：Map 的渲染尺寸为 0，空气墙不生效");
            return;
        }

        mapBounds = b;
        hasMapBounds = true;
    }

    private void ClampToMap()
    {
        if (!hasMapBounds) return;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, mapBounds.min.x + mapMargin, mapBounds.max.x - mapMargin);
        p.y = Mathf.Clamp(p.y, mapBounds.min.y + mapMargin, mapBounds.max.y - mapMargin);
        transform.position = p;
    }

    private void ApplyRolePassive()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/role");
        if (jsonFile == null)
        {
            Debug.LogError("找不到角色数据: Resources/Data/role.json");
            return;
        }
        List<RoleData> roles = JsonConvert.DeserializeObject<List<RoleData>>(jsonFile.text);

        RoleData role = roles.Find(r => r.id == GameData.selectedRoleId);
        if (role == null) return;

        if (!string.IsNullOrEmpty(role.avatar) && sr != null)
        {
            Sprite roleSprite = Resources.Load<Sprite>(role.avatar);
            if (roleSprite != null)
                sr.sprite = roleSprite;
        }

        PassiveData p = role.passive;
        if (p == null) return;

        maxHp += p.maxHp;
        hp += p.maxHp;

        speed *= 1f + p.speedPer;

        WeaponBase weapon = FindObjectOfType<WeaponBase>();
        if (weapon != null)
            weapon.AddDamagePer(p.damagePer);

        GamePanel.Instance?.RenewHp();
    }

    void Update()
    {
        if (isDead) return;
        Move();
        Regenerate();
    }

    private void Regenerate()
    {
        if (hpRegen == 0f) return;

        float before = hp;
        hp = Mathf.Min(maxHp, hp + hpRegen * Time.deltaTime);

        if (hp <= 0f)
        {
            hp = 0f;
            Dead();
            return;
        }

        if (Mathf.FloorToInt(before) != Mathf.FloorToInt(hp))
            GamePanel.Instance?.RenewHp();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(h, v);
        movement.Normalize();
        transform.Translate(movement * speed * Time.deltaTime);
        ClampToMap();

        if (sr != null && Mathf.Abs(h) > 0.01f)
            sr.flipX = h < 0;
    }

    public void Injured(float attack)
    {
        if (isDead) return;
        StartCoroutine(FlashRed());
        AudioManager.Instance?.PlaySound("受伤音效");

        hp -= attack;
        if (hp <= 0)
        {

            hp = 0;
            Dead();
        }

        GamePanel.Instance?.RenewHp();
    }

    public void Dead()
    {
        isDead = true;
        if (anim != null)
            anim.speed = 0;
        ResultPanel.Instance?.Show(false);
    }

    public void AddExp(int amount)
    {
        exp += amount * expMultiplier;

        while (exp >= needExp)
        {
            exp -= needExp;
            level++;
            needExp = Mathf.RoundToInt(needExp * 1.3f);
            pendingLevelUps++;
        }

        GamePanel.Instance?.RenewExp();

        if (pendingLevelUps > 0 &&
            (UpgradePanel.Instance == null || !UpgradePanel.Instance.IsShowing))
        {
            ShowUpgrade();
        }
    }
    public void  AddMoney(int amount)
    {
        money += amount;

        GamePanel.Instance?.RenewMoney();
    }

    private void ShowUpgrade()
    {
        if (UpgradePanel.Instance == null)
        {
            Debug.LogError("场景里没有挂 UpgradePanel 组件！");
            return;
        }
        Time.timeScale = 0;
        UpgradePanel.Instance.Show();
    }

    public void CompleteLevelUp()
    {
        pendingLevelUps--;
        if (pendingLevelUps > 0)
        {
            UpgradePanel.Instance.Show();
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void AddMaxHp(float amount)
    {

        maxHp = Mathf.Max(1f, maxHp + amount);
        hp = Mathf.Min(hp + amount, maxHp);

        if (hp <= 0f)
        {
            hp = 0f;
            Dead();
        }

        GamePanel.Instance?.RenewHp();
    }

    public void AddSpeed(float amount)
    {
        speed += amount;
    }

    public void AddExpMultiplier(float amount)
    {
        expMultiplier += amount;
    }

    public void AddShopDiscount(float amount)
    {
        shopDiscount += amount;
    }

    public void AddPickRangePer(float per)
    {
        pickRange *= 1f + per;
    }

    public void AddHpRegen(float amount)
    {
        hpRegen += amount;
    }

    public void AddAttack(float amount)
    {
        WeaponBase weapon = FindObjectOfType<WeaponBase>();
        if (weapon != null)
            weapon.AddDamage(amount);
    }

    public void SpawnWeapon()
    {

        WeaponData data = GetSelectedWeaponData();
        if (data == null)
        {
            Debug.LogError("武器数据缺失，无法生成武器");
            return;
        }

        GameObject go = new GameObject("Weapon");
        go.transform.SetParent(transform);

        go.transform.localPosition = data.isLong == 1 ? new Vector2(0.9f, 0f) : new Vector2(0.55f, 0f);

        SpriteRenderer weaponSr = go.AddComponent<SpriteRenderer>();
        weaponSr.sprite = Resources.Load<Sprite>(data.avatar);
        weaponSr.sortingLayerName = "Player";
        weaponSr.sortingOrder = 1;

        if (data.isLong == 1)
        {
            Pistol pistol = go.AddComponent<Pistol>();
            pistol.InitFromData(data);
            pistol.SetBulletSprite(BulletSpriteFor(data.id));
        }
        else
        {
            MeleeWeapon melee = go.AddComponent<MeleeWeapon>();
            melee.InitFromData(data);
        }
    }

    private WeaponData GetSelectedWeaponData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/weapon");
        if (jsonFile == null)
        {
            Debug.LogError("找不到武器数据: Resources/Data/weapon.json");
            return null;
        }
        List<WeaponData> weapons = JsonConvert.DeserializeObject<List<WeaponData>>(jsonFile.text);
        WeaponData data = weapons.Find(w => w.id == GameData.selectedWeaponId);
        if (data == null)
            data = weapons.Find(w => w.id == 4);
        return data;
    }

    private Sprite BulletSpriteFor(int weaponId)
    {
        string path = "Image/武器/手枪子弹";
        if (weaponId == 3) path = "Image/武器/医疗枪子弹";
        else if (weaponId == 5) path = "Image/武器/弓箭";
        return Resources.Load<Sprite>(path);
    }
    IEnumerator FlashRed()
    {
        sr.color = new Color(1f, 0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = originColor;
    }
}
