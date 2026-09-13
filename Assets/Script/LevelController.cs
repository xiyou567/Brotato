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

    private Dictionary<string, EnemyData> enemyDataCache = new Dictionary<string, EnemyData>();

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

        if (FindObjectOfType<ShopPanel>() == null)
            gameObject.AddComponent<ShopPanel>();
    }

    void Start()
    {

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

    private IEnumerator SpawnAtTimeAxis(WaveEnemyData spawnData)
    {
        yield return new WaitForSeconds(spawnData.timeAxis);

        for (int i = 0; i < spawnData.count; i++)
        {

            SpawnEnemy(spawnData.enemyName,i < spawnData.elite);

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
