using UnityEngine;

/// <summary>
/// 탑다운 이동 스크립트. 중력 없이 상하좌우(8방향) 이동.
/// Rigidbody2D의 Gravity Scale은 0으로 설정해야 함 (Inspector에서 직접 0으로 변경).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // 공격 중 이동을 잠그고 싶을 때 외부에서 제어
    public bool CanMove { get; set; } = true;

    // 마지막으로 바라본 방향 (공격 방향 등에 사용)
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = moveInput;
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = CanMove ? moveInput * moveSpeed : Vector2.zero;
        rb.linearVelocity = targetVelocity;
    }
}