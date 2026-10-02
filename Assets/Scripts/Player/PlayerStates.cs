using UnityEngine;
// Стан спокою
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.CurrentMovementState = PlayerControllerState.WALKING;
        player.SwapMaterial(player.normalMaterial);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.isCrouching && player.IsGrounded)
        {
            stateMachine.ChangeState(player.CrouchState);
        }
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
        player.CurrentMovementState = PlayerControllerState.AIR;
        player.SwapMaterial(player.normalMaterial);
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
        if (player.InputHandler.isCrouching && !player.IsGrounded)
        {
            stateMachine.ChangeState(player.CrouchState);
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
        float facingDirection = Mathf.Sign(player.transform.localScale.x);
        if (player.IsTouchingWall && 
            Mathf.Abs(player.InputHandler.normalizedInputX) > 0.1f && 
            Mathf.Sign(player.InputHandler.normalizedInputX) == facingDirection)
        {
            stateMachine.ChangeState(player.WallClimbState);
            return; // Важливо додати return, щоб код нижче не виконувався після зміни стану
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

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.CurrentMovementState = PlayerControllerState.WALKING;
        player.SwapMaterial(player.normalMaterial);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.JumpInput && player.IsGrounded)
        {
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        else if (player.InputHandler.isCrouching && player.IsGrounded)
        {
            stateMachine.ChangeState(player.CrouchState);
        }
        else if (Mathf.Abs(player.InputHandler.normalizedInputX) < 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
        float facingDirection = Mathf.Sign(player.transform.localScale.x);
        if (player.IsTouchingWall && 
            Mathf.Abs(player.InputHandler.normalizedInputX) > 0.1f && 
            Mathf.Sign(player.InputHandler.normalizedInputX) == facingDirection)
        {
            stateMachine.ChangeState(player.WallClimbState);
            return; // Важливо додати return, щоб код нижче не виконувався після зміни стану
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

public class PlayerCrouchState : PlayerState
{
    public PlayerCrouchState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.CurrentMovementState = PlayerControllerState.WALKING;
        player.SwapMaterial(player.normalMaterial);
        player.transform.localScale = new Vector3(player.transform.localScale.x, 0.5f, player.transform.localScale.z);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. Перехід зі стану: якщо гравець ВІДПУСТИВ кнопку присідання
        if (!player.InputHandler.isCrouching)
        {
            if (!player.IsGrounded)
            {
                // Якщо відпустив у повітрі -> повертаємося в стан стрибка/падіння
                stateMachine.ChangeState(player.JumpState);
            }
            else if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.01f)
            {
                // Якщо на землі і тисне рух -> йдемо
                stateMachine.ChangeState(player.WalkState);
            }
            else
            {
                // Якщо на землі і стоїть -> стан спокою
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.IsGrounded)
        {
            // Рух по землі (Повзання) зі зменшеною швидкістю
            float crouchSpeed = player.data.movementVelocity * player.data.crouchSpeedMultiplier;
            player.Rigidbody.linearVelocity = new Vector2(
                player.InputHandler.normalizedInputX * crouchSpeed,
                player.Rigidbody.linearVelocity.y
            );
        }
        else
        {
            // У повітрі (Fast Fall)
            // Дозволяємо керувати вліво-вправо (Air Control) під час швидкого падіння
            float targetSpeedX = player.InputHandler.normalizedInputX * player.data.movementVelocity;
            float smoothedSpeedX = Mathf.Lerp(
                player.Rigidbody.linearVelocity.x,
                targetSpeedX,
                player.data.airControl * Time.fixedDeltaTime
            );

            // Примусово задаємо сильну швидкість вниз
            player.Rigidbody.linearVelocity = new Vector2(
                smoothedSpeedX,
                player.data.fastFallSpeed
            );
        }

        FlipController(player.InputHandler.normalizedInputX);
    }

    public override void Exit()
    {
        base.Exit();
        // Повертаємо нормальну висоту персонажа при виході зі стану
        player.transform.localScale = new Vector3(player.transform.localScale.x, 1f, player.transform.localScale.z);
    }

    private void FlipController(float inputX)
    {
        // Зверніть увагу: ми зберігаємо поточний Y масштаб, щоб персонаж не "виростав" при розвороті
        if (inputX > 0)
            player.transform.localScale = new Vector3(1, player.transform.localScale.y, 1);
        else if (inputX < 0)
            player.transform.localScale = new Vector3(-1, player.transform.localScale.y, 1);
    }
}

public class PlayerWallClimbState : PlayerState
{
    public PlayerWallClimbState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.CurrentMovementState = PlayerControllerState.CLIMBING;
        player.SwapMaterial(player.climbMaterial);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.InputHandler.JumpInput)
        {
            player.InputHandler.UseJumpInput();

            // Визначаємо напрямок стрибка (у протилежний бік від стіни)
            float jumpDirection = -Mathf.Sign(player.transform.localScale.x);

            // Скидаємо поточну швидкість перед застосуванням сили
            player.Rigidbody.linearVelocity = Vector2.zero;
            player.Rigidbody.AddForce(new Vector2(jumpDirection * player.data.wallJumpForce.x, player.data.wallJumpForce.y), ForceMode2D.Impulse);

            // Розворот гравця
            player.transform.localScale = new Vector3(jumpDirection, player.transform.localScale.y, 1);

            stateMachine.ChangeState(player.JumpState);
            return;
        }
        if (player.IsGrounded)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (!player.IsTouchingWall)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }
        float facingDirection = Mathf.Sign(player.transform.localScale.x);
        if (Mathf.Abs(player.InputHandler.normalizedInputX) > 0.1f &&
            Mathf.Sign(player.InputHandler.normalizedInputX) != facingDirection)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.Rigidbody.linearVelocity = new Vector2(
            0f, 
            player.InputHandler.normalizedInputY * player.data.wallClimbVelocity
        );
    }
}