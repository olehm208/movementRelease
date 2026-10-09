using UnityEngine;
// Стан падіння
public class PlayerFallState : PlayerState
{
    // Гравітація до входу в падіння, щоб повернути її при виході
    private float defaultGravityScale;

    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        // Посилюємо гравітацію, щоб падіння було швидшим за підйом
        defaultGravityScale = player.Rigidbody.gravityScale;
        player.Rigidbody.gravityScale = defaultGravityScale * player.data.fallGravityMultiplier;
    }

    public override void Exit()
    {
        base.Exit();

        // Повертаємо звичайну гравітацію
        player.Rigidbody.gravityScale = defaultGravityScale;
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

        // Стрибок у повітрі, якщо стрибки ще лишилися
        if (player.InputHandler.JumpInput)
        {
            player.InputHandler.UseJumpInput();

            if (player.JumpState.CanJump())
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }
        }

        // При приземленні повертаємося в Idle або Walk
        if (player.IsGrounded)
        {
            // Після приземлення знову доступні всі стрибки
            player.JumpState.ResetJumps();

            if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
            {
                stateMachine.ChangeState(player.WalkState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Рух у повітрі — так само, як у стані стрибка
        float targetSpeedX = player.InputHandler.normalizedInputX * player.data.movementVelocity;
        float smoothedSpeedX;

        // Якщо кнопка руху натиснута — керування через airControl
        if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
        {
            smoothedSpeedX = Mathf.Lerp(
                player.Rigidbody.linearVelocity.x,
                targetSpeedX,
                player.data.airControl * Time.fixedDeltaTime
            );
        }
        // Якщо кнопку відпущено — плавно гасимо інерцію через airDeceleration
        else
        {
            smoothedSpeedX = Mathf.MoveTowards(
                player.Rigidbody.linearVelocity.x,
                0f,
                player.data.airDeceleration * Time.fixedDeltaTime
            );
        }

        player.Rigidbody.linearVelocity = new Vector2(smoothedSpeedX, player.Rigidbody.linearVelocity.y);

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
