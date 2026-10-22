using UnityEngine;
// Стан ривка
public class PlayerDashState : PlayerState
{
    // Коли почався поточний ривок
    private float dashStartTime;
    // Коли закінчився попередній ривок (для перезарядки)
    private float lastDashTime = -999f;
    // Напрям ривка: 1 — вправо, -1 — вліво
    private float dashDirection;
    // Гравітація до ривка, щоб повернути її при виході
    private float defaultGravityScale;
    // Чи почався ривок на землі (щоб правильно порахувати стрибки, якщо злетіли з краю)
    private bool startedOnGround;

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Чи минула перезарядка
    public bool CanDash()
    {
        return Time.time >= lastDashTime + player.data.dashCooldown;
    }

    public override void Enter()
    {
        base.Enter();

        dashStartTime = Time.time;
        startedOnGround = player.IsGrounded;

        // Напрям: куди натиснуто, а якщо нічого — куди дивиться персонаж
        float inputX = player.InputHandler.normalizedInputX;
        if (Mathf.Abs(inputX) > 0.01f)
            dashDirection = Mathf.Sign(inputX);
        else
            dashDirection = Mathf.Sign(player.transform.localScale.x);

        // Розвертаємо спрайт у бік ривка
        player.transform.localScale = new Vector3(dashDirection, 1, 1);

        // Вимикаємо гравітацію, щоб ривок був строго горизонтальним
        defaultGravityScale = player.Rigidbody.gravityScale;
        player.Rigidbody.gravityScale = 0f;
    }

    public override void Exit()
    {
        base.Exit();

        // Повертаємо гравітацію і запускаємо перезарядку
        player.Rigidbody.gravityScale = defaultGravityScale;
        lastDashTime = Time.time;

        // Після ривка лишаємо звичайну швидкість бігу, щоб персонаж не ковзав далеко
        player.Rigidbody.linearVelocity = new Vector2(dashDirection * player.data.movementVelocity, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Натиснуто Dash -> ривок, якщо минула перезарядка
        if (player.InputHandler.DashInput)
        {
            player.InputHandler.UseDashInput();

            if (player.DashState.CanDash())
            {
                stateMachine.ChangeState(player.DashState);
                return;
            }
        }

        // Ривок закінчився
        if (Time.time >= dashStartTime + player.data.dashDuration)
        {
            if (player.IsGrounded)
            {
                if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
                    stateMachine.ChangeState(player.WalkState);
                else
                    stateMachine.ChangeState(player.IdleState);
            }
            else
            {
                // Злетіли з краю ривком із землі — наземний стрибок втрачено
                if (startedOnGround)
                    player.JumpState.UseGroundJump();

                stateMachine.ChangeState(player.FallState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Постійна швидкість ривка, без вертикального руху
        player.Rigidbody.linearVelocity = new Vector2(dashDirection * player.data.dashSpeed, 0f);
    }
}
