using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 12f;
    [SerializeField, Min(0f)] private float lifetime = 3f;
    [SerializeField, Min(0)] private int damage = 10;
    [SerializeField, Min(0f)] private float hitRadius = 0.15f;
    [SerializeField] private LayerMask hitLayers;

    private float moveDirection = 1f;

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * (moveDirection * speed * Time.deltaTime);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, hitRadius, hitLayers);

        if (hit == null)
        {
            return;
        }

        Health health = hit.GetComponentInParent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    public void Launch(float horizontalDirection)
    {
        if (!Mathf.Approximately(horizontalDirection, 0f))
        {
            moveDirection = Mathf.Sign(horizontalDirection);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * moveDirection;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
