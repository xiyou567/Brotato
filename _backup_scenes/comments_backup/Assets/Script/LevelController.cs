using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("关卡配置")]
    [SerializeField] private string levelName = "level0";
    [SerializeField] private float spawnDistance = 10f;

    private List<WaveData> waves;
    private int currentWaveIndex;
    private float waveCountdown;
    private bool isWaveActive;

    // 敌人数据缓存，避免每帧 Resources.Load
    private Dictionary<string, EnemyData> enemyDataCache = new Dictionary<string, EnemyData>();

    // 公开属性给 GamePanel 读取
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => waves != null ? waves.Count : 0;
    public float WaveCountdown => waveCountdown;
    public bool IsWaveActive => isWaveActive;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 场景里没挂 ShopPanel 就运行时补一个，否则波次之间那段商店逻辑永远进不去
        // （ShopPanel.Instance 恒为 null）。走 AddComponent 而不是挂到 Inspector 上，
        // 因为这个脚本的 meta guid 不是 32 位 hex，写进场景会解析报错。
        // 只让单例那条路建，否则 ShopPanel 的 Awake 会把宿主物体（就是本物体）销毁。
        if (FindObjectOfType<ShopPanel>() == null)
            gameObject.AddComponent<ShopPanel>();
    }

    void Start()
    {
        // 从 GameData 读玩家在选关场景选择的难度关卡名（没选则用 Inspector 默认 level0）
        if (!string.IsNullOrEmpty(GameData.selectedLevelName))
            levelName = GameData.selectedLevelName;

        LoadLevelData();
        LoadEnemyData();
        if (waves != null && waves.Count > 0)
        {
            StartCoroutine(WaveLoop());
        }
        
        Debug.Log("读档：最高波次=" + SaveManager.bestWave
                             + " 总击杀=" + SaveManager.totalKill
                             + " 总金币=" + SaveManager.totalMoney
                             + " 局数=" + SaveManager.playCount);
    }

    /// <summary>从 enemy.json 加载敌人基础数据到缓存</summary>
    private void LoadEnemyData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/enemy");
        if (jsonFile == null)
        {
            Debug.LogError("找不到敌人数据: Resources/Data/enemy.json");
            return;
        }
        List<EnemyData> list = JsonConvert.DeserializeObject<List<EnemyData>>(jsonFile.text);
        foreach (EnemyData e in list)
        {
            enemyDataCache[e.name] = e;
        }
        Debug.Log("加载敌人数据，共 " + list.Count + " 种");
    }

    void Update()
    {
        if (isWaveActive && waveCountdown > 0)
        {
            waveCountdown -= Time.deltaTime;
            GamePanel.Instance?.RenewCountDown(waveCountdown);
        }
    }

    /// <summary>从 Resources/Data/ 加载关卡 JSON</summary>
    private void LoadLevelData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/" + levelName);
        if (jsonFile == null)
        {
            Debug.LogError("找不到关卡数据: Resources/Data/" + levelName + ".json");
            return;
        }
        waves = JsonConvert.DeserializeObject<List<WaveData>>(jsonFile.text);
        Debug.Log("加载关卡 " + levelName + "，共 " + waves.Count + " 波");
    }

    /// <summary>主波次循环：逐波推进</summary>
    private IEnumerator WaveLoop()
    {
        while (currentWaveIndex < waves.Count)
        {
            WaveData currentWave = waves[currentWaveIndex];
            waveCountdown = currentWave.waveTimer;
            isWaveActive = true;

            GamePanel.Instance?.RenewWave(currentWaveIndex + 1, waves.Count);

            foreach (WaveEnemyData spawn in currentWave.enemys)
            {
                StartCoroutine(SpawnAtTimeAxis(spawn));
            }

            while (waveCountdown > 0)
            {
                yield return null;
            }

            isWaveActive = false;
            currentWaveIndex++;

            // 不是最后一波 → 暂停并弹商店，等玩家买完点「继续」再开下一波
            if (currentWaveIndex < waves.Count && ShopPanel.Instance != null)
            {
                Time.timeScale = 0;
                ShopPanel.Instance.Show();
                while (ShopPanel.Instance.IsShowing)
                {
                    yield return null;
                }
                Time.timeScale = 1;
            }
        }

        Debug.Log("所有波次完成，胜利！");
        ResultPanel.Instance?.Show(true);
    }

    /// <summary>在 timeAxis 指定的时刻刷出一批敌人</summary>
    private IEnumerator SpawnAtTimeAxis(WaveEnemyData spawnData)
    {
        yield return new WaitForSeconds(spawnData.timeAxis);

        for (int i = 0; i < spawnData.count; i++)
        {

            SpawnEnemy(spawnData.enemyName,i < spawnData.elite);
            // 同时间点大量生成时稍微错开，防止堆叠
            if (spawnData.count > 10)
                yield return new WaitForSeconds(0.03f);
        }
    }

    private void SpawnEnemy(string enemyName, bool isElite)
    {
        if (!enemyDataCache.TryGetValue(enemyName, out EnemyData data))
        {
            Debug.LogError("找不到敌人数据: " + enemyName);
            return;
        }

        Vector2 spawnPos = GetRandomSpawnPos();
        // 走对象池复用，避免每波大量 new GameObject
        if (EnemyPool.Instance == null)
        {
            Debug.LogError("场景里没有 EnemyPool！敌人刷不出来");
            return;
        }
        EnemyPool.Instance.Get(data, spawnPos, isElite);
    }

    private Vector2 GetRandomSpawnPos()
    {
        if (Player.Instance == null) return Vector2.zero;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        return (Vector2)Player.Instance.transform.position + offset;
    }
}
