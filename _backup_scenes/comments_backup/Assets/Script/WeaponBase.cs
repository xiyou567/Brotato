using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器基类：自动瞄准 + 攻击冷却 + 旋转指向 + 子弹生成
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("武器属性")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooling = 1f;            // 与 weapon.json 的 cooling 字段对齐
    [SerializeField] protected float range = 4f;
    [SerializeField] protected float rotationSpeed = 15f;
    [SerializeField] protected float rangePixelScale = 0.01f; // JSON 里的 range 是像素值

    [Header("暴击")]
    [SerializeField] protected float criticalChance;
    [SerializeField] protected float criticalMultiple = 2f;

    [Header("子弹配置")]
    [SerializeField] protected Sprite bulletSprite;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float bulletLifetime = 3f;

    private bool isLong;

    protected float attackTimer;
    protected Transform currentTarget;

    protected virtual void Awake()
    {
        attackTimer = 0f;
    }

    /// <summary>从 weapon.json 读入属性</summary>
    public void InitFromData(WeaponData data)
    {
        damage = data.damage;
        cooling = data.cooling;
        range = data.range * rangePixelScale;
        isLong = data.isLong == 1;
        criticalChance = data.critical_strikes_probability;
        criticalMultiple = data.critical_strikes_multiple;
    }

    /// <summary>远程为 true、近战为 false，道具加成要按这个分长短</summary>
    public bool IsLong => isLong;

    public void AddDamage(float amount)
    {
        damage += amount;
    }

    public void AddDamagePer(float per)
    {
        damage *= 1f + per;
    }

    /// <summary>攻速 +per（0.1 = 快 10%）。攻速是冷却的倒数，所以是除不是乘</summary>
    public void AddAttackSpeedPer(float per)
    {
        if (per <= -1f) return;          // 会把冷却除成 0 或负数
        cooling /= 1f + per;
    }

    /// <summary>攻击范围 +per（0.2 = +20%）</summary>
    public void AddRangePer(float per)
    {
        range = Mathf.Max(0.1f, range * (1f + per));
    }

    public void AddCritChance(float amount)
    {
        criticalChance = Mathf.Clamp01(criticalChance + amount);
    }

    /// <summary>每次命中掷一次暴击，返回本次实际伤害</summary>
    protected float RollDamage()
    {
        if (criticalChance > 0f && Random.value < criticalChance)
            return damage * criticalMultiple;

        return damage;
    }

    protected virtual void Update()
    {
        FindTarget();
        RotateTowardTarget();
        HandleAttack();
    }

    protected virtual void FindTarget()
    {
        // 目标没死且还在范围内就不换，否则武器会来回甩
        if (currentTarget != null)
        {
            float distToCurrent = Vector2.Distance(transform.position, currentTarget.position);
            if (distToCurrent <= range) return;
        }

        currentTarget = null;
        float closestDist = Mathf.Infinity;

        // 敌人数量多了以后每帧全量遍历开销偏大，可改用 Physics2D.OverlapCircle 先筛范围
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist && dist <= range)
            {
                closestDist = dist;
                currentTarget = enemy.transform;
            }
        }
    }

    protected virtual void RotateTowardTarget()
    {
        if (currentTarget == null) return;

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime
        );
    }

    protected virtual void HandleAttack()
    {
        if (currentTarget == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = cooling;
            Fire();
        }
    }

    /// <summary>开火，子类可重写实现特殊武器</summary>
    protected virtual void Fire()
    {
        if (currentTarget == null) return;

        Vector2 direction = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        SpawnBullet(direction, RollDamage());
        AudioManager.Instance?.PlaySound("射击音效");
    }

    protected void SpawnBullet(Vector2 direction, float dmg)
    {
        BulletPool.Instance.Get(bulletSprite, dmg, bulletSpeed, direction, bulletLifetime, transform.position);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public void SetBulletSprite(Sprite sprite)
    {
        bulletSprite = sprite;
    }

}
