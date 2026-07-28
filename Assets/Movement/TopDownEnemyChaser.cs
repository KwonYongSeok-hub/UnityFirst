using UnityEngine;

/// <summary>
/// 탑다운용 몬스터 AI: 플레이어를 8방향으로 추적하다가 사거리에 들어오면 공격.
/// Rigidbody2D(Gravity Scale 0) + Collider2D + Health가 붙은 Enemy GameObject에 부착.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownEnemyChaser : MonoBehaviour
{
    [Header("추적 설정")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectRange = 6f;

    [Header("공격 설정")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;

    private Rigidbody2D rb;
    private Transform player;
    private float cooldownTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack();
        }
        else if (distance <= detectRange)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;

        if (player.TryGetComponent<Health>(out Health playerHealth))
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
