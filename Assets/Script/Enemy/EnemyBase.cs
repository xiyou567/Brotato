using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("基础属性")]
    [SerializeField] protected float hp = 10;
    [SerializeField] protected float speed = 3;
    [SerializeField] protected float damage = 10;
    [SerializeField] protected float attackTime = 1;
    [SerializeField] protected int provideExp = 1;

    [Header("移动设置")]
    [SerializeField] protected float stopDistance = 0.003f;

    protected float attackTimer;
    protected bool isContact;
    protected bool isCooling;

    private GameObject moneyPrefab;

    public bool IsTargetable => hp > 0 && gameObject.activeInHierarchy;

    public void InitFromData(EnemyData data)
    {
        hp = data.hp;
        speed = data.speed;
        damage = data.damage;
        attackTime = data.attackTime;
        provideExp = data.provideExp;

        isContact = false;
        isCooling = false;
        attackTimer = 0;
        transform.localScale = Vector3.one;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && !string.IsNullOrEmpty(data.sprite))
        {
            Sprite sprite = Resources.Load<Sprite>(data.sprite);
            if (sprite != null)
                sr.sprite = sprite;
        }
    }

    private void Awake()
    {
        moneyPrefab = Resources.Load<GameObject>("Prefabs/Money");
    }

    private void Update()
    {
        Move();
        TurnAround();

        if (isContact && !isCooling)
        {
            Attack();
        }

        if (isCooling)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                attackTimer = 0;
                isCooling = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isContact = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isContact = false;
        }
    }

    protected virtual void Move()
    {
        if (Player.Instance == null) return;

        Vector2 toPlayer = Player.Instance.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance < 0.001f) return;

        Vector2 direction = toPlayer / distance;
        Vector2 moveAmount = direction * speed * Time.deltaTime;
        transform.Translate(moveAmount);
    }

    protected virtual void TurnAround()
    {
        if (Player.Instance == null) return;

        float direction = Player.Instance.transform.position.x - transform.position.x;

        if (Mathf.Abs(direction) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void Attack()
    {
        if (isCooling) return;
        if (Player.Instance == null) return;

        Player.Instance.Injured(damage);
        AudioManager.Instance?.PlaySound("攻击音效");
        isCooling = true;
        attackTimer = attackTime;
    }

    public void TakeDamage(float dmg)
    {

        if (hp <= 0) return;

        hp -= dmg;
        DamageNumber.Show(transform.position, dmg);
        if (hp <= 0)
        {
            if (moneyPrefab != null)
            {
                Instantiate(moneyPrefab, transform.position, Quaternion.identity);
            }

            if (Player.Instance != null)
            {
                Player.Instance.AddExp(provideExp);
                Player.Instance.killCount++;
            }

            if (EnemyPool.Instance != null)
            {
                EnemyPool.Instance.Return(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void ApplyElite()
    {
        hp+=4*hp;
        transform.localScale = new Vector3(transform.localScale.x*1.5f, transform.localScale.y*1.5f, transform.localScale.z);

    }
}
