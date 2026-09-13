using UnityEngine;

/// <summary>
/// 角色解锁判定。role.json 里 unlock 是写死的结果，conditionType/conditionValue 才是规则，
/// 每次进选角场景重新算一遍，算出来是解锁就覆盖掉那个 0。
/// </summary>
public static class UnlockChecker
{
    public static bool IsUnlocked(RoleData r)
    {
        if (r == null) return false;
        if (r.unlock == 1) return true;   // JSON 里直接给了解锁的，比如前三个

        SaveManager.EnsureLoaded();

        switch (r.conditionType)
        {
            case "playCount":
                return SaveManager.playCount >= r.conditionValue;
            case "maxHp":
                return SaveManager.maxHpReached >= r.conditionValue;
            default:
                Debug.LogWarning("角色「" + r.name + "」的条件没有对应规则，暂时锁着：" + r.unlockConditions);
                return false;
        }
    }
}
