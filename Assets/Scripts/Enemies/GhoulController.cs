using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class GhoulController : MonoBehaviour
{
    [Header("Гравець")]
    [SerializeField] private Transform player;

    [Header("Рух")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Вибух")]
    [SerializeField] private float explosionRange = 1.2f;
    [SerializeField] private int damage = 3;
    [SerializeField] private float explosionDuration = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private CombatHealth playerHealth;

    private bool chasing;
    private bool exploded;
    private float direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Перетягни Player у поле Player гуля!", this);
            enabled = false;
            return;
        }

        playerHealth = player.GetComponent<CombatHealth>();

        if (playerHealth == null)
            Debug.LogError("На Player немає CombatHealth!", player);
    }

    private void Update()
    {
        if (exploded || player == null)
            return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            chasing = false;
            animator.speed = 0;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= explosionRange)
        {
            Explode();
            return;
        }

        chasing = distance <= detectionRange;
        animator.speed = chasing ? 1 : 0;

        if (chasing)
        {
            direction = Mathf.Sign(player.position.x - transform.position.x);

            if (spriteRenderer != null && direction != 0)
            {
                bool shouldFaceRight = direction > 0;
                spriteRenderer.flipX = shouldFaceRight != spriteFacesRight;
            }
        }
    }

    private void FixedUpdate()
    {
        float speedX = chasing && !exploded ? direction * moveSpeed : 0;
        rb.linearVelocity = new Vector2(speedX, rb.linearVelocity.y);
    }

    private void Explode()
    {
        exploded = true;
        chasing = false;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.speed = 1;
        animator.SetTrigger("Explode");

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        Destroy(gameObject, explosionDuration);
    }
}