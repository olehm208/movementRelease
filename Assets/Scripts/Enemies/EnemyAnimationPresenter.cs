
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimationPresenter : MonoBehaviour
{
    [SerializeField] private CharacterAnimationAPI animationAPI;
    [SerializeField] private float movementThreshold = 0.05f;

    private EnemyController enemy;
    private Rigidbody2D rb;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();

        if (animationAPI == null)
            animationAPI = GetComponentInChildren<CharacterAnimationAPI>(true);
    }

    private void Update()
    {
        if (animationAPI == null || animationAPI.IsDead)
            return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > movementThreshold;

        CharacterMotionState motion;

        if (!isMoving)
        {
            motion = CharacterMotionState.Idle;
        }
        else if (enemy.IsChasing)
        {
            motion = CharacterMotionState.Chase;
        }
        else
        {
            motion = CharacterMotionState.Move;
        }

        animationAPI.SetMotion(motion);
    }

    public void PlayHurt()
    {
        animationAPI?.PlayHurt();
    }

    public void PlayDeath()
    {
        if (animationAPI == null || animationAPI.IsDead)
            return;

        animationAPI.PlayDeath();

        enemy.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
