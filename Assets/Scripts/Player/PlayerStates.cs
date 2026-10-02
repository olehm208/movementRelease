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
        if (Input.GetButtonDown("Jump"))
        {
            stateMachine.ChangeState(player.JumpState);
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
        player.Rigidbody.AddForce(Vector3.up * player.data.jumpForce, ForceMode.Impulse);
    }

    public override void LogicUpdate() 
    {
        base.LogicUpdate();

        // При приземленні повертаємося в Idle
        if (player.IsGrounded && player.Rigidbody.linearVelocity.y <= 0) 
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}