using UnityEngine;
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerData data;
    public PlayerInput InputHandler { get; private set; }
    // Список станів
    public PlayerIdleState IdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerMoveState WalkState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerDashState DashState { get; private set; }

    public Rigidbody2D Rigidbody { get; private set; }
    public Transform GroundCheck; 
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        InputHandler = GetComponent<PlayerInput>();
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        WalkState = new PlayerMoveState(this, StateMachine);
        FallState = new PlayerFallState(this, StateMachine);
        DashState = new PlayerDashState(this, StateMachine);
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
        CheckIfGrounded();
    }
    private void CheckIfGrounded()
    {
        // Перевіряємо, чи є в заданому радіусі об'єкти з шаром "Земля"
        IsGrounded = Physics2D.OverlapCircle(GroundCheck.position, data.groundCheckRadius, data.whatIsGround);
    }
    private void OnDrawGizmos()
    {
        if (GroundCheck != null && data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheck.position, data.groundCheckRadius);
        }
    }
}