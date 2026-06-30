using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpForce = 8f;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Visual")]
    [SerializeField] private GameObject shape;

    [Header("Invulnerability")]
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 1.5f;
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

    [Header("Game Over")]
    [SerializeField] private float fallDeathY = -10f;

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
    private Vector2 movementInput;
    private Health health;
    private float nextFireTime;
    private bool jumpRequested;
    private bool isGrounded;
    private float shapeScaleX;
    private float facingDirection = 1f;
    private SpriteRenderer[] shapeRenderers;
    private bool isRestarting;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        health.OnDeath.AddListener(OnDeath);
        health.OnDamaged.AddListener(StartInvulnerability);

        if (shape != null)
        {
            shapeScaleX = Mathf.Abs(shape.transform.localScale.x);
            shapeRenderers = shape.GetComponentsInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnDeath()
    {
        RestartScene();
    }

    private void StartInvulnerability()
    {
        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        health.SetInvulnerable(true);
        float endTime = Time.time + invulnerabilityDuration;
        bool isVisible = true;

        while (Time.time < endTime)
        {
            isVisible = !isVisible;
            SetShapeVisible(isVisible);
            yield return new WaitForSeconds(blinkInterval);
        }

        SetShapeVisible(true);
        health.SetInvulnerable(false);
    }

    private void SetShapeVisible(bool isVisible)
    {
        if (shapeRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in shapeRenderers)
        {
            spriteRenderer.enabled = isVisible;
        }
    }

    private void Update()
    {
        if (transform.position.y <= fallDeathY)
        {
            RestartScene();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        movementInput = new Vector2(horizontal, 0f);

        FlipShape(horizontal);

        Vector2 groundCheckOrigin = transform.TransformPoint(groundCheckOffset);
        isGrounded = Physics2D.Raycast(
            groundCheckOrigin,
            Vector2.down,
            groundCheckDistance,
            groundLayers).collider != null;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        UpdateMovementAnimation();
    }

    private void RestartScene()
    {
        if (isRestarting)
        {
            return;
        }

        isRestarting = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void FlipShape(float horizontal)
    {
        if (shape == null || Mathf.Approximately(horizontal, 0f))
        {
            return;
        }

        Vector3 scale = shape.transform.localScale;
        facingDirection = Mathf.Sign(horizontal);
        scale.x = shapeScaleX * facingDirection;
        shape.transform.localScale = scale;
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
        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity);

        if (bullet.TryGetComponent(out Bullet projectile))
        {
            projectile.Launch(facingDirection);
        }

        SetTriggerIfValid(shootParameter);
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
        Vector3 origin = transform.TransformPoint(groundCheckOffset);
        Vector3 end = origin + Vector3.down * groundCheckDistance;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, end);
    }
}
