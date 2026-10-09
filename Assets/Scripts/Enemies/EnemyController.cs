
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float stoppingDistance = 1.5f;

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

        Move(Mathf.Sign(distance));
    }

    private void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
