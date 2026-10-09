using UnityEngine;
// Стан спокою
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

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

        // Стрілка вниз + стрибок на односторонній платформі -> зістрибуємо крізь неї
        if (player.InputHandler.JumpInput && player.IsGrounded
            && player.InputHandler.normalizedInputY < -0.5f
            && player.DownJumpState.CanDownJump())
        {
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.DownJumpState);
            return;
        }

        // Якщо натиснуто пробіл -> переходимо в стан стрибка
        if (player.InputHandler.JumpInput && player.IsGrounded)
        {
            // Обов'язково "використовуємо" інпут, щоб скинути прапорець
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        // Під ногами немає землі -> падаємо, наземний стрибок втрачено
        else if (!player.IsGrounded)
        {
            player.JumpState.UseGroundJump();
            stateMachine.ChangeState(player.FallState);
        }
        else if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
        {
            stateMachine.ChangeState(player.WalkState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Плавно зменшуємо горизонтальну швидкість до 0
        float newSpeedX = Mathf.MoveTowards(
            player.Rigidbody.linearVelocity.x,
            0f,
            player.data.groundDeceleration * Time.fixedDeltaTime
        );

        player.Rigidbody.linearVelocity = new Vector2(newSpeedX, player.Rigidbody.linearVelocity.y);
    }
}
