using UnityEngine;

public static class UnlockChecker
{
    public static bool IsUnlocked(RoleData r)
    {
        if (r == null) return false;
        if (r.unlock == 1) return true;

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
