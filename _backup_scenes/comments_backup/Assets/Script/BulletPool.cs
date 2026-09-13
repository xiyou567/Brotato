using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹对象池 — 复用子弹 GameObject，避免频繁 Instantiate/Destroy 造成 GC 卡顿
/// </summary>
public class BulletPool : MonoBehaviour
{
    // 必须挂在场景物体上，场景里没有这个物体的话 Instance 一直是 null
    public static BulletPool Instance;

    [Header("预热数量：开局先造好一批子弹")]
    [SerializeField] private int prewarmCount = 20;

    private Queue<Bullet> pool = new Queue<Bullet>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 预热：避免开局第一次发射时集中创建
        for (int i = 0; i < prewarmCount; i++)
        {
            Bullet bullet = CreateBullet();
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    /// <summary>取一个子弹，并一次性设置好位置 / 朝向 / 贴图 / 飞行参数</summary>
    public Bullet Get(Sprite sprite, float damage, float speed, Vector2 direction, float lifetime, Vector2 position)
    {
        Bullet bullet;
        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
        }
        else
        {
            bullet = CreateBullet();
        }

        bullet.transform.position = position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        bullet.SetSprite(sprite);
        bullet.Init(damage, speed, direction, lifetime);
        return bullet;
    }

    public void Return(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }

    private Bullet CreateBullet()
    {
        GameObject obj = new GameObject("Bullet");

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        // 触发器也需要 Rigidbody2D，否则 OnTriggerEnter2D 收不到
        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        return obj.AddComponent<Bullet>();
    }
}
