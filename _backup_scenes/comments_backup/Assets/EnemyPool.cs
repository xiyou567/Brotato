using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [Header("预热数量")]
    [SerializeField] private int prewarmCount = 20;

    private Queue<EnemyBase> pool = new Queue<EnemyBase>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;   // 关键：销毁后不能再往下预热
        }

        // 预热：避免开局第一波集中创建
        for (int i = 0; i < prewarmCount; i++)
        {
            EnemyBase enemy = CreateEnemy();
            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    public EnemyBase Get(EnemyData data, Vector2 position, bool isElite)
    {
        EnemyBase enemy;
        if (pool.Count > 0)
        {
            enemy = pool.Dequeue();
            enemy.gameObject.SetActive(true);
        }
        else
        {
            enemy = CreateEnemy();
        }

        // InitFromData 里会重置 hp / 速度 / scale 等
        enemy.transform.position = position;
        enemy.InitFromData(data);
        if (isElite)
        {
            enemy.ApplyElite();
        }
        return enemy;
    }

    public void Return(EnemyBase enemy)
    {
        // 重置状态放在 InitFromData 里，下次 Get 时统一处理
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }

    private EnemyBase CreateEnemy()
    {
        // 只造空壳，数据初始化交给 Get 里的 InitFromData
        GameObject go = new GameObject("Enemy");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        return go.AddComponent<EnemyBase>();
    }
}
