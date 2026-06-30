using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 100;

    [Header("Shooting")]
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.2f;

    [Header("Animation Parameters")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runParameter = "IsRunning";
    [SerializeField] private string idleParameter = "IsIdle";
    [SerializeField] private string shootParameter = "Shoot";

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private Rigidbody2D body;
    private Camera mainCamera;
    private Vector2 movementInput;
    private float nextFireTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        CurrentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShoot;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (shootAction != null)
        {
            shootAction.action.performed -= OnShoot;
            shootAction.action.Disable();
        }
    }

    private void Update()
    {
        movementInput = moveAction == null
            ? Vector2.zero
            : moveAction.action.ReadValue<Vector2>().normalized;

        UpdateMovementAnimation();
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + movementInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.time < nextFireTime || bulletPrefab == null)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        Vector3 spawnPosition = firePoint == null ? transform.position : firePoint.position;
        Vector2 direction = GetAimDirection(spawnPosition);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, angle));

        if (bullet.TryGetComponent(out Bullet projectile))
        {
            projectile.Launch(direction);
        }

        SetTriggerIfValid(shootParameter);
    }

    private Vector2 GetAimDirection(Vector3 spawnPosition)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || Mouse.current == null)
        {
            return transform.right;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 direction = mouseWorldPosition - spawnPosition;

        return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.right;
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = movementInput.sqrMagnitude > 0.01f;
        SetBoolIfValid(runParameter, isMoving);
        SetBoolIfValid(idleParameter, !isMoving);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount > 0 && CurrentHealth > 0)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }
    }

    private void Die()
    {
        movementInput = Vector2.zero;
        gameObject.SetActive(false);
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
}
