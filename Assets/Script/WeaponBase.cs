using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("武器属性")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooling = 1f;
    [SerializeField] protected float range = 4f;
    [SerializeField] protected float rotationSpeed = 15f;
    [SerializeField] protected float rangePixelScale = 0.01f;

    [Header("暴击")]
    [SerializeField] protected float criticalChance;
    [SerializeField] protected float criticalMultiple = 2f;

    [Header("子弹配置")]
    [SerializeField] protected Sprite bulletSprite;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float bulletLifetime = 3f;

    private bool isLong;

    protected float attackTimer;
    protected EnemyBase currentTarget;

    protected bool HasTarget => currentTarget != null && currentTarget.IsTargetable;

    protected virtual void Awake()
    {
        attackTimer = 0f;
    }

    public void InitFromData(WeaponData data)
    {
        damage = data.damage;
        cooling = data.cooling;
        range = data.range * rangePixelScale;
        isLong = data.isLong == 1;
        criticalChance = data.critical_strikes_probability;
        criticalMultiple = data.critical_strikes_multiple;
    }

    public bool IsLong => isLong;

    public void AddDamage(float amount)
    {
        damage += amount;
    }

    public void AddDamagePer(float per)
    {
        damage *= 1f + per;
    }

    public void AddAttackSpeedPer(float per)
    {
        if (per <= -1f) return;
        cooling /= 1f + per;
    }

    public void AddRangePer(float per)
    {
        range = Mathf.Max(0.1f, range * (1f + per));
    }

    public void AddCritChance(float amount)
    {
        criticalChance = Mathf.Clamp01(criticalChance + amount);
    }

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
        if (HasTarget)
        {
            float distToCurrent = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (distToCurrent <= range) return;
        }

        currentTarget = null;
        float closestDist = Mathf.Infinity;

        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null || !enemy.IsTargetable) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist && dist <= range)
            {
                closestDist = dist;
                currentTarget = enemy;
            }
        }
    }

    protected virtual void RotateTowardTarget()
    {
        if (!HasTarget) return;

        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime
        );
    }

    protected virtual void HandleAttack()
    {
        if (!HasTarget) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = cooling;
            Fire();
        }
    }

    protected virtual void Fire()
    {
        if (!HasTarget) return;

        Vector2 direction = ((Vector2)currentTarget.transform.position - (Vector2)transform.position).normalized;
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
