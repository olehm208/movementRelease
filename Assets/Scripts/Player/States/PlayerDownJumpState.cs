using UnityEngine;

// Стан зістрибування крізь платформу
public class PlayerDownJumpState : PlayerState
{
    // Колайдер гравця
    private Collider2D playerCollider;
    // Платформа, крізь яку проходимо
    private Collider2D platformCollider;

    public PlayerDownJumpState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine)
    {
        playerCollider = player.GetComponent<Collider2D>();
    }

    // Чи стоїть гравець на платформі, крізь яку можна зістрибнути
    public bool CanDownJump()
    {
        Collider2D ground = Physics2D.OverlapCircle(
            player.GroundCheck.position,
            player.data.groundCheckRadius,
            player.data.whatIsGround
        );

        // Зістрибнути можна тільки з односторонньої платформи (з Platform Effector 2D)
        if (ground != null && ground.GetComponent<PlatformEffector2D>() != null)
        {
            platformCollider = ground;
            return true;
        }

        return false;
    }

    public override void Enter()
    {
        base.Enter();

        // Вимикаємо зіткнення гравця саме з цією платформою
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        // Як і при сході з краю — наземний стрибок втрачено
        player.JumpState.UseGroundJump();
    }

    public override void Exit()
    {
        base.Exit();

        // Повертаємо зіткнення, щоб на платформу знову можна було стати
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

        public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Гравець повністю під платформою
        bool isBelowPlatform = playerCollider.bounds.max.y < platformCollider.bounds.min.y;

        // Ноги вже нижче платформи і стоять на іншій землі (якщо під платформою мало місця)
        bool feetPassedPlatform = playerCollider.bounds.min.y < platformCollider.bounds.min.y;
        bool isOnOtherGround = feetPassedPlatform && IsStandingOnOtherGround();

        if (isBelowPlatform || isOnOtherGround)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }

    // Чи стоїть гравець на будь-якій землі, крім платформи, крізь яку проходить
    private bool IsStandingOnOtherGround()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            player.GroundCheck.position,
            player.data.groundCheckRadius,
            player.data.whatIsGround
        );

        foreach (Collider2D hit in hits)
        {
            if (hit != platformCollider)
                return true;
        }

        return false;
    }
}