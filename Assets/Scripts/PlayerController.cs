using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpForce = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0f)] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.2f;

    [Header("Animation Parameters")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runParameter = "IsRunning";
    [SerializeField] private string idleParameter = "IsIdle";
    [SerializeField] private string shootParameter = "Shoot";

    private Rigidbody2D body;
    private Camera mainCamera;
    private Vector2 movementInput;
    private float nextFireTime;
    private bool jumpRequested;
    private bool isGrounded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        movementInput = new Vector2(horizontal, 0f);

        Vector3 groundCheckPosition = groundCheck == null
            ? transform.position
            : groundCheck.position;

        isGrounded = Physics2D.OverlapCircle(
            groundCheckPosition,
            groundCheckRadius,
            groundLayers) != null;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        UpdateMovementAnimation();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;
        velocity.x = movementInput.x * moveSpeed;

        if (jumpRequested)
        {
            velocity.y = jumpForce;
            jumpRequested = false;
        }

        body.linearVelocity = velocity;
    }

    private void Shoot()
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

        if (mainCamera == null)
        {
            return transform.right;
        }

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
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
        Vector3 position = groundCheck == null ? transform.position : groundCheck.position;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(position, groundCheckRadius);
    }
}
