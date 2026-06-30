using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField] private GameObject visual;
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    [Header("Ground Detection")]
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private Vector2 frontGroundCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Contact Damage")]
    [SerializeField, Min(0)] private int contactDamage = 10;
    [SerializeField, Min(0.01f)] private float damageCooldown = 1f;
    [SerializeField] private Vector2 playerCheckOffset;
    [SerializeField, Min(0f)] private float playerCheckRadius = 0.5f;
    [SerializeField] private LayerMask playerLayers;

    [Header("Animation Parameters")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveParameter = "IsMoving";

    private Health ownHealth;
    private Rigidbody2D body;
    private float direction;
    private float visualScaleX;
    private float nextDamageTime;
    private bool hasBeenGrounded;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
        body = GetComponent<Rigidbody2D>();
        direction = Random.value < 0.5f ? -1f : 1f;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (visual == null && animator != null)
        {
            visual = animator.gameObject;
        }

        if (visual != null)
        {
            visualScaleX = Mathf.Abs(visual.transform.localScale.x);
            SetRandomSprite();
            UpdateVisualDirection();
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

    private void FixedUpdate()
    {
        if (ownHealth.IsDead)
        {
            return;
        }

        if (!hasBeenGrounded && IsGrounded())
        {
            hasBeenGrounded = true;
        }

        if (hasBeenGrounded && !HasGroundAhead())
        {
            TurnAround();
        }

        body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocity.y);
        SetBoolIfValid(moveParameter, true);
        TryDamagePlayer();
    }

    private bool IsGrounded()
    {
        Vector2 origin = transform.TransformPoint(groundCheckOffset);

        return Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayers).collider != null;
    }

    private bool HasGroundAhead()
    {
        Vector2 localOffset = frontGroundCheckOffset;
        localOffset.x = Mathf.Abs(localOffset.x) * direction;
        Vector2 origin = transform.TransformPoint(localOffset);

        return Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayers).collider != null;
    }

    private void TurnAround()
    {
        direction *= -1f;
        UpdateVisualDirection();
    }

    private void UpdateVisualDirection()
    {
        if (visual == null)
        {
            return;
        }

        Vector3 scale = visual.transform.localScale;
        scale.x = visualScaleX * direction;
        visual.transform.localScale = scale;
    }

    private void SetRandomSprite()
    {
        if (sprites == null || sprites.Count == 0
            || !visual.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            return;
        }

        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
    }

    private void TryDamagePlayer()
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        Vector2 checkPosition = transform.TransformPoint(playerCheckOffset);
        Collider2D playerCollider = Physics2D.OverlapCircle(
            checkPosition,
            playerCheckRadius,
            playerLayers);

        if (playerCollider == null)
        {
            return;
        }

        PlayerController player = playerCollider.GetComponentInParent<PlayerController>();

        if (player != null && player.TryGetComponent(out Health playerHealth))
        {
            nextDamageTime = Time.time + damageCooldown;
            playerHealth.TakeDamage(contactDamage);
        }
    }

    private void HandleDeath()
    {
        body.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    private void SetBoolIfValid(string parameterName, bool value)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float previewDirection = Mathf.Approximately(direction, 0f) ? 1f : direction;
        Vector2 localOffset = frontGroundCheckOffset;
        localOffset.x = Mathf.Abs(localOffset.x) * previewDirection;
        Vector3 origin = transform.TransformPoint(localOffset);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);

        Vector3 groundOrigin = transform.TransformPoint(groundCheckOffset);
        Gizmos.color = hasBeenGrounded ? Color.green : Color.yellow;
        Gizmos.DrawLine(
            groundOrigin,
            groundOrigin + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.TransformPoint(playerCheckOffset),
            playerCheckRadius);
    }
}
