using UnityEngine;
// Стан спокою
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Якщо натиснуто пробіл -> переходимо в стан стрибка
        if (player.InputHandler.JumpInput && player.IsGrounded)
        {
            // Обов'язково "використовуємо" інпут, щоб скинути прапорець
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        else if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
        {
            stateMachine.ChangeState(player.WalkState);
        }
    }
}

// Стан стрибка
public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.Rigidbody.AddForce(Vector3.up * player.data.jumpForce, ForceMode2D.Impulse);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Захист від фантомного стрибка
        if(player.InputHandler.JumpInput)
        {
            player.InputHandler.UseJumpInput();
        }

        // При приземленні повертаємося в Idle або Walk
        if (player.IsGrounded && player.Rigidbody.linearVelocity.y <= 0)
        {
            // Якщо гравець продовжує тиснути кнопку руху — переходимо в біг
            if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
            {
                stateMachine.ChangeState(player.WalkState);
            }
            // Якщо кнопки відпущені — переходимо у спокій
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 1. Обчислюємо цільову швидкість
        float targetSpeedX = player.InputHandler.normalizedInputX * player.data.movementVelocity;

        // 2. Згладжуємо швидкість для ефекту інерції (air control)
        float smoothedSpeedX = Mathf.Lerp(
            player.Rigidbody.linearVelocity.x,
            targetSpeedX,
            player.data.airControl * Time.fixedDeltaTime
        );

        // 3. Застосовуємо обчислену швидкість до гравця
        player.Rigidbody.linearVelocity = new Vector2(
            smoothedSpeedX,
            player.Rigidbody.linearVelocity.y
        );

        // 4. Розворот спрайту в польоті
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

public class PlayerMoveState: PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }


    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.JumpInput && player.IsGrounded)
        {
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
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