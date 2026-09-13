using UnityEngine;

public static class SaveManager
{
    public static int bestWave;
    public static int totalKill;
    public static int totalMoney;
    public static int playCount;
    public static int maxHpReached;

    private static bool loaded;

    private const string KEY_BEST_WAVE   = "Brotato_BestWave";
    private const string KEY_TOTAL_KILL  = "Brotato_TotalKill";
    private const string KEY_TOTAL_MONEY = "Brotato_TotalMoney";
    private const string KEY_PLAY_COUNT = "Brotato_PlayCount";
    private const string KEY_MAX_HP      = "Brotato_MaxHpReached";

    public static void EnsureLoaded()
    {
        if (!loaded) LoadAll();
    }

    public static void LoadAll()
    {
        bestWave   = PlayerPrefs.GetInt(KEY_BEST_WAVE, 0);
        totalKill  = PlayerPrefs.GetInt(KEY_TOTAL_KILL, 0);
        totalMoney = PlayerPrefs.GetInt(KEY_TOTAL_MONEY, 0);
        playCount = PlayerPrefs.GetInt(KEY_PLAY_COUNT, 0);
        maxHpReached = PlayerPrefs.GetInt(KEY_MAX_HP, 0);
        loaded = true;
    }

    public static void NoteMaxHp(int maxHp)
    {
        if (maxHp <= maxHpReached) return;

        maxHpReached = maxHp;
        PlayerPrefs.SetInt(KEY_MAX_HP, maxHpReached);
        PlayerPrefs.Save();
    }

    public static void SaveResult(int wave, int kill, int money)
    {

        EnsureLoaded();

        bestWave = Mathf.Max(bestWave, wave);

        totalKill  += kill;
        totalMoney += money;
        playCount += 1;

        PlayerPrefs.SetInt(KEY_BEST_WAVE, bestWave);
        PlayerPrefs.SetInt(KEY_TOTAL_KILL, totalKill);
        PlayerPrefs.SetInt(KEY_TOTAL_MONEY, totalMoney);
        PlayerPrefs.SetInt(KEY_PLAY_COUNT, playCount);

        PlayerPrefs.Save();
    }
}
