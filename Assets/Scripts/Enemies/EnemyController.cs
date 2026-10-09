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
    [Tooltip("Наскільки гравець може бути вище/нижче ворога, щоб той його помітив (щоб не бачив інші поверхи)")]
    public float maxDetectionHeight = 1.5f;

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

    // Чи вже помітив гравця (тоді бачить його і за спиною)
    private bool isChasing;
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
        if (player && CanSeePlayer())
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

        // Зона, в якій ворог помічає гравця попереду
        Gizmos.color = new Color(1f, 0.5f, 0f);
        float gizmoFacing = Mathf.Sign(transform.localScale.x);
        Vector3 visionCenter = transform.position + Vector3.right * gizmoFacing * detectionRange / 2f;
        Gizmos.DrawWireCube(visionCenter, new Vector3(detectionRange, maxDetectionHeight * 2f, 0f));

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


    // Чи бачить ворог гравця: в радіусі, на своєму поверсі, без стін між ними
    // Ще не помічений гравець має бути попереду, а вже помічений — будь-де
    private bool CanSeePlayer()
    {
        Vector2 toPlayer = player.transform.position - transform.position;

        // Задалеко
        if (toPlayer.magnitude > detectionRange)
        {
            isChasing = false;
            return false;
        }

        // На іншому поверсі
        if (Mathf.Abs(toPlayer.y) > maxDetectionHeight)
        {
            isChasing = false;
            return false;
        }

        // Між ворогом і гравцем стіна або платформа
        if (Physics2D.Linecast(transform.position, player.transform.position, whatIsGround))
        {
            isChasing = false;
            return false;
        }

        // Ще не помітив — бачить тільки те, що попереду
        if (!isChasing)
        {
            float facing = Mathf.Sign(transform.localScale.x);

            if (Mathf.Sign(toPlayer.x) != facing)
                return false;

            isChasing = true;
        }

        return true;
    }
}