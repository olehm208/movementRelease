using UnityEngine;

public class PlayerMoveState: PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine)
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

        if (player.InputHandler.JumpInput && player.IsGrounded)
        {
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        // Зійшли з краю платформи -> падаємо, наземний стрибок втрачено
        else if (!player.IsGrounded)
        {
            player.JumpState.UseGroundJump();
            stateMachine.ChangeState(player.FallState);
        }
        else if (Mathf.Abs(player.InputHandler.normalizedInputX) < 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Рух через швидкість (рекомендовано для платформерів)
        // Задаємо горизонтальну швидкість, залишаючи вертикальну (гравітацію) без змін
        player.Rigidbody.linearVelocity = new Vector2(
                player.InputHandler.normalizedInputX * player.data.movementVelocity,
                player.Rigidbody.linearVelocity.y
            );

        // Розворот спрайту гравця в бік руху
        FlipController(player.InputHandler.normalizedInputX);
    }
    private void FlipController(float inputX)
    {
        if (inputX > 0)
            player.transform.localScale = new Vector3(1, 1, 1);
        else if (inputX < 0)
            player.transform.localScale = new Vector3(-1, 1, 1);
    }
}
