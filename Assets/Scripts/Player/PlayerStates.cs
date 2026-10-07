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

// Стан стрибка
public class PlayerJumpState : PlayerState
{
    // Скільки стрибків ще можна зробити до приземлення
    private int jumpsLeft;

    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine)
    {
        ResetJumps();
    }

    public void ResetJumps()
    {
        jumpsLeft = player.data.amountOfJumps;
    }

    public override void Enter()
    {
        base.Enter();
        jumpsLeft--;

        // Задаємо вертикальну швидкість напряму, а не через AddForce:
        // 1) кожен стрибок однаковий, навіть якщо гравець уже падає;
        // 2) швидкість змінюється одразу, а не на наступному кроці фізики.
        player.Rigidbody.linearVelocity = new Vector2(
            player.Rigidbody.linearVelocity.x,
            player.data.jumpForce
        );
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Захист від фантомного стрибка
        // Якщо стрибки ще лишилися — це стрибок у повітрі (Double Jump)
        if (player.InputHandler.JumpInput)
        {
            player.InputHandler.UseJumpInput();

            if (jumpsLeft > 0)
            {
                // Перезапуск стану -> Exit() і Enter() -> новий поштовх
                stateMachine.ChangeState(player.JumpState);
                return;
            }
        }

        // При приземленні повертаємося в Idle або Walk
        if (player.IsGrounded && player.Rigidbody.linearVelocity.y <= 0)
        {
            // Після приземлення знову доступні всі стрибки
            ResetJumps();

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
        float smoothedSpeedX;

        // якщо кнопка руху натиснута — керування через airControl, як раніше
        if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
        {
            smoothedSpeedX = Mathf.Lerp(
                player.Rigidbody.linearVelocity.x,
                targetSpeedX,
                player.data.airControl * Time.fixedDeltaTime
            );
        }
        // якщо кнопку відпущено — плавно гасимо інерцію через airDeceleration
        else
        {
            smoothedSpeedX = Mathf.MoveTowards(
                player.Rigidbody.linearVelocity.x,
                0f,
                player.data.airDeceleration * Time.fixedDeltaTime
            );
        }

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