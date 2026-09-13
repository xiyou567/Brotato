using UnityEngine;

public class Money : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    [SerializeField] private float magnetSpeed = 9f;

    private void Update()
    {
        if (Player.Instance == null) return;

        float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (dist > Player.Instance.pickRange) return;

        transform.position = Vector2.MoveTowards(
            transform.position, Player.Instance.transform.position, magnetSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player") return;
        if (Player.Instance == null) return;

        Player.Instance.AddMoney(amount);
        Destroy(gameObject);
    }
}
