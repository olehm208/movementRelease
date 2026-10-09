using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float stoppingDistance = 1.5f;

    [Header("Environment Checks")]
    [Tooltip("Точка перед ворогом на рівні ніг — звідси перевіряємо, чи є земля попереду")]
    public Transform ledgeCheck;
    [Tooltip("Точка перед ворогом на рівні тіла — звідси перевіряємо, чи немає стіни попереду")]
    public Transform wallCheck;
    [Tooltip("Як глибоко вниз шукати землю перед ворогом")]
    public float ledgeCheckDistance = 0.5f;
    [Tooltip("Як далеко вперед шукати стіну")]
    public float wallCheckDistance = 0.2f;
    [Tooltip("Шари, які вважаються землею і стінами")]
    public LayerMask whatIsGround;

    private Rigidbody2D rb;
    private int patrolIndex;
    private PlayerController player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        player = PlayerController.Instance;
    }

    private void FixedUpdate()
    {
        if (player && Vector2.Distance(transform.position, player.transform.position) <= detectionRange)
        {
            ChasePlayer(player.transform);
        }
        else
        {
            Patrol();
        }
    }

    private void ChasePlayer(Transform player)
    {
        float distance = Mathf.Abs(player.position.x - transform.position.x);

        if (distance <= stoppingDistance)
        {
            Move(0f);
            return;
        }

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Якщо попереду край або стіна — Move() сам зупинить ворога, і він чекатиме на краю
        Move(direction);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Move(0f);
            return;
        }

        Transform point = patrolPoints[patrolIndex];

        if (point == null)
        {
            Move(0f);
            return;
        }

        float distance = point.position.x - transform.position.x;

        if (Mathf.Abs(distance) <= 0.3f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            return;
        }

        // Шлях до точки перекритий краєм або стіною — розвертаємось до наступної точки
        if (!Move(Mathf.Sign(distance)))
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    // Повертає false, якщо рухатись не вдалося через край або стіну
    private bool Move(float direction)
    {
        if (direction != 0f)
        {
            Flip(direction);

            if (IsPathBlocked())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return false;
            }
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        return true;
    }

    // Розвертаємо ворога в бік руху, зберігаючи його розмір
    private void Flip(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
        transform.localScale = scale;
    }

    // Чи є попереду прірва або стіна
    private bool IsPathBlocked()
    {
        float facing = Mathf.Sign(transform.localScale.x);

        bool isGroundAhead = true;
        if (ledgeCheck != null)
            isGroundAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, ledgeCheckDistance, whatIsGround);

        bool isWallAhead = false;
        if (wallCheck != null)
            isWallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * facing, wallCheckDistance, whatIsGround);

        return !isGroundAhead || isWallAhead;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Промені перевірки краю і стіни
        Gizmos.color = Color.cyan;
        if (ledgeCheck != null)
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * ledgeCheckDistance);

        if (wallCheck != null)
        {
            float facing = Mathf.Sign(transform.localScale.x);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * facing * wallCheckDistance);
        }
    }
}