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

        // Пройшли верхню точку стрибка — починаємо падати
        if (!player.IsGrounded && player.Rigidbody.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.FallState);
            return;
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

    // Чи можна ще стрибнути (для стану падіння)
    public bool CanJump()
    {
        return jumpsLeft > 0;
    }

    // Зійшли з краю без стрибка — наземний стрибок втрачено
    public void UseGroundJump()
    {
        jumpsLeft--;
    }
}

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