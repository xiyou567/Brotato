using UnityEngine;

/// <summary>
/// 子弹 — 飞行 + 碰撞伤害 + 超时归还对象池
/// </summary>
public class Bullet : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;

    private float lifetime;
    private float lifeTimer;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>每次从池取出后调用，重置所有飞行参数</summary>
    public void Init(float damage, float speed, Vector2 direction, float lifetime = 3f)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction;
        this.lifetime = lifetime;
        this.lifeTimer = 0f;   // 关键：不复位的话复用的子弹会立刻超时
    }

    /// <summary>不同武器贴图不同，取出时覆盖</summary>
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 超时归还（原来是 Destroy(gameObject, lifetime)）
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            ReturnToPool();
        }
    }

    /// <summary>命中、超时都走这里</summary>
    private void ReturnToPool()
    {
        // 已经归还过（失活）就直接返回，防止同一颗子弹被重复塞进池
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.Return(this);
        }
        else
        {
            Destroy(gameObject); // 兜底：池不存在时退回销毁
        }
    }
}
