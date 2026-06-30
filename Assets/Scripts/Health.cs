using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private UnityEvent onDamaged = new UnityEvent();
    [SerializeField] private UnityEvent onDeath = new UnityEvent();

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public bool IsInvulnerable { get; private set; }
    public UnityEvent OnDamaged => onDamaged;
    public UnityEvent OnDeath => onDeath;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvulnerable)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        onDamaged.Invoke();

        if (CurrentHealth == 0)
        {
            IsDead = true;
            onDeath.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount > 0 && !IsDead)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        IsInvulnerable = false;
    }

    public void SetInvulnerable(bool isInvulnerable)
    {
        IsInvulnerable = isInvulnerable && !IsDead;
    }
}
