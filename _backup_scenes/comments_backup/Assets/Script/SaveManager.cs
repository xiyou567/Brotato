using UnityEngine;

/// <summary>
/// 存档管理器：跨局数据进游戏时从硬盘读一次进静态缓存，
/// 之后各界面统一从这儿拿，别再到处 GetInt，保证取值一致。
/// </summary>
public static class SaveManager
{
    public static int bestWave;
    public static int totalKill;
    public static int totalMoney;
    public static int playCount;
    public static int maxHpReached;   // 历史单局最大生命值峰值，给角色解锁判定用

    private static bool loaded;

    // 带 Brotato_ 前缀，避免和其他项目的 key 撞车
    private const string KEY_BEST_WAVE   = "Brotato_BestWave";
    private const string KEY_TOTAL_KILL  = "Brotato_TotalKill";
    private const string KEY_TOTAL_MONEY = "Brotato_TotalMoney";
    private const string KEY_PLAY_COUNT = "Brotato_PlayCount";
    private const string KEY_MAX_HP      = "Brotato_MaxHpReached";

    /// <summary>直接从选角场景启动时没人调 LoadAll，这里兜一次</summary>
    public static void EnsureLoaded()
    {
        if (!loaded) LoadAll();
    }

    /// <summary>进游戏调一次：硬盘 → 缓存</summary>
    public static void LoadAll()
    {
        bestWave   = PlayerPrefs.GetInt(KEY_BEST_WAVE, 0);
        totalKill  = PlayerPrefs.GetInt(KEY_TOTAL_KILL, 0);
        totalMoney = PlayerPrefs.GetInt(KEY_TOTAL_MONEY, 0);
        playCount = PlayerPrefs.GetInt(KEY_PLAY_COUNT, 0);
        maxHpReached = PlayerPrefs.GetInt(KEY_MAX_HP, 0);
        loaded = true;
    }

    /// <summary>一局结束时记一次生命值峰值，只增不减</summary>
    public static void NoteMaxHp(int maxHp)
    {
        if (maxHp <= maxHpReached) return;

        maxHpReached = maxHp;
        PlayerPrefs.SetInt(KEY_MAX_HP, maxHpReached);
        PlayerPrefs.Save();
    }

    /// <summary>一局结算调一次：更新纪录/累计 → 写回 → 落盘</summary>
    public static void SaveResult(int wave, int kill, int money)
    {
        // 没读过档就直接累加，会把硬盘上的旧纪录冲成 0 起步
        EnsureLoaded();

        // 最高纪录只增不减
        bestWave = Mathf.Max(bestWave, wave);

        totalKill  += kill;
        totalMoney += money;
        playCount += 1;
        // 先改缓存再 SetInt，两处才一致
        PlayerPrefs.SetInt(KEY_BEST_WAVE, bestWave);
        PlayerPrefs.SetInt(KEY_TOTAL_KILL, totalKill);
        PlayerPrefs.SetInt(KEY_TOTAL_MONEY, totalMoney);
        PlayerPrefs.SetInt(KEY_PLAY_COUNT, playCount);
        // Unity 默认退出才落盘，这里主动存，防崩溃丢数据
        PlayerPrefs.Save();
    }
}