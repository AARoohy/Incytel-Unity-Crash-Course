using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement and Attack")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField, Min(0f)] private float attackDistance = 1.25f;
    [SerializeField, Min(0.01f)] private float attackCooldown = 1f;
    [SerializeField, Min(0)] private int attackDamage = 10;

    [Header("Animation Parameters")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleParameter = "IsIdle";
    [SerializeField] private string moveParameter = "IsMoving";
    [SerializeField] private string attackParameter = "Attack";

    private Health ownHealth;
    private Health targetHealth;
    private Rigidbody2D body;
    private float nextAttackTime;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
        body = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        ownHealth.OnDeath.AddListener(HandleDeath);
    }

    private void OnDisable()
    {
        ownHealth.OnDeath.RemoveListener(HandleDeath);
    }

    private void Start()
    {
        if (target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                SetTarget(player.transform);
            }
        }
        else
        {
            SetTarget(target);
        }
    }

    private void Update()
    {
        if (target == null || ownHealth.IsDead)
        {
            StopMoving();
            SetMovementAnimation(false);
            return;
        }

        Vector2 offset = target.position - transform.position;
        float distance = offset.magnitude;

        if (distance > attackDistance)
        {
            float horizontalDirection = Mathf.Sign(offset.x);
            body.linearVelocity = new Vector2(
                horizontalDirection * moveSpeed,
                body.linearVelocity.y);
            SetMovementAnimation(Mathf.Abs(offset.x) > 0.05f);
        }
        else
        {
            StopMoving();
            SetMovementAnimation(false);
            TryAttack();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetHealth = target == null ? null : target.GetComponentInParent<Health>();
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        SetTriggerIfValid(attackParameter);

        if (targetHealth != null && !targetHealth.IsDead)
        {
            targetHealth.TakeDamage(attackDamage);
        }
    }

    private void HandleDeath()
    {
        StopMoving();
        Destroy(gameObject);
    }

    private void StopMoving()
    {
        if (body != null)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }

    private void SetMovementAnimation(bool isMoving)
    {
        if (animator == null)
        {
            return;
        }

        SetBoolIfValid(moveParameter, isMoving);
        SetBoolIfValid(idleParameter, !isMoving);
    }

    private void SetBoolIfValid(string parameterName, bool value)
    {
        if (!string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetTriggerIfValid(string parameterName)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetTrigger(parameterName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
