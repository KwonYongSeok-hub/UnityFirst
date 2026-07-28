using UnityEngine;

/// <summary>
/// 탑다운 검 공격. 마우스가 가리키는 방향으로 공격 판정이 생김 (자동공격 없음, 클릭 입력 기반).
/// Player GameObject에 TopDownMovement와 함께 부착.
/// </summary>
public class TopDownCombat : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDistance = 0.8f; // 캐릭터 중심에서 공격 판정까지 거리
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    private Camera mainCamera;
    private float cooldownTimer;
    private Vector2 aimDirection = Vector2.down;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        UpdateAimDirection();

        if (Input.GetButtonDown("Fire1") && cooldownTimer <= 0f)
        {
            Attack();
            cooldownTimer = attackCooldown;
        }
    }

    private void UpdateAimDirection()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position);

        if (direction.sqrMagnitude > 0.01f)
        {
            aimDirection = direction.normalized;
        }
    }

    private void Attack()
    {
        Vector2 attackPoint = (Vector2)transform.position + aimDirection * attackDistance;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Health>(out Health targetHealth))
            {
                targetHealth.TakeDamage(attackDamage);
            }
        }

        // TODO: 공격 방향으로 슬래시 애니메이션/이펙트 재생
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 attackPoint = (Vector2)transform.position + aimDirection * attackDistance;
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }
}
