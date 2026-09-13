using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    protected override void FindTarget()
    {

        Transform anchor = Player.Instance != null ? Player.Instance.transform : transform;

        if (HasTarget &&
            Vector2.Distance(anchor.position, currentTarget.transform.position) <= range)
        {
            return;
        }

        currentTarget = null;
        float closestDist = Mathf.Infinity;

        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null || !enemy.IsTargetable) continue;

            float dist = Vector2.Distance(anchor.position, enemy.transform.position);
            if (dist < closestDist && dist <= range)
            {
                closestDist = dist;
                currentTarget = enemy;
            }
        }
    }

    protected override void RotateTowardTarget()
    {
    }

    protected override void Fire()
    {
        if (!HasTarget) return;

        currentTarget.TakeDamage(RollDamage());
        AudioManager.Instance?.PlaySound("攻击音效");
    }
}
