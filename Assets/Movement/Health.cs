using System;
using UnityEngine;

/// <summary>
/// 플레이어와 몬스터가 공통으로 사용하는 체력 컴포넌트.
/// PlayerCombat, 몬스터 공격 스크립트 등에서 TakeDamage()를 호출.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // 마을 강화 등으로 최대체력이 바뀔 때 사용
    public void SetMaxHealth(float newMax, bool healToFull = true)
    {
        maxHealth = newMax;
        if (healToFull) CurrentHealth = maxHealth;
        else CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        OnDied?.Invoke();
        // TODO: 플레이어면 던전 이탈/마을 귀환 로직, 몬스터면 파괴+드랍 로직 연결
    }
}
