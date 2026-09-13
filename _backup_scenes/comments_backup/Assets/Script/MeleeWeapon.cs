using UnityEngine;

/// <summary>
/// 近战武器（如拳）— 不射子弹，冷却好了直接打锁定目标一下。
/// 找目标时以【玩家中心】为圆心算距离：武器握在手上带偏移，
/// 否则站在玩家左侧的怪会因为在武器"背后"而永远打不到。
/// </summary>
public class MeleeWeapon : WeaponBase
{
    protected override void FindTarget()
    {
        // 玩家没就绪时退回武器自身位置兜底
        Transform anchor = Player.Instance != null ? Player.Instance.transform : transform;

        // 目标还在范围内就保持锁定，避免每帧换目标
        if (currentTarget != null &&
            Vector2.Distance(anchor.position, currentTarget.position) <= range)
        {
            return;
        }

        currentTarget = null;
        float closestDist = Mathf.Infinity;

        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = Vector2.Distance(anchor.position, enemy.transform.position);
            if (dist < closestDist && dist <= range)
            {
                closestDist = dist;
                currentTarget = enemy.transform;
            }
        }
    }

    // 近战不旋转指向目标，保持原朝向
    protected override void RotateTowardTarget()
    {
    }

    protected override void Fire()
    {
        if (currentTarget == null) return;

        EnemyBase enemy = currentTarget.GetComponent<EnemyBase>();
        if (enemy == null) return;

        enemy.TakeDamage(RollDamage());        // 扣血/掉钱/给经验/回池都在 TakeDamage 里
        AudioManager.Instance?.PlaySound("攻击音效");
    }
}
