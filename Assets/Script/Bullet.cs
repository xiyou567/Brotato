using UnityEngine;

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

    public void Init(float damage, float speed, Vector2 direction, float lifetime = 3f)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction;
        this.lifetime = lifetime;
        this.lifeTimer = 0f;
    }

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

    private void ReturnToPool()
    {

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
            Destroy(gameObject);
        }
    }
}
