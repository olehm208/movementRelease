using UnityEngine;

public enum PlayerControllerState
{
    WALKING,
    CLIMBING,
    AIR
}
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerControllerState CurrentMovementState { get; set; }

    [Header("Physics Materials")]
    public PhysicsMaterial2D normalMaterial;
    public PhysicsMaterial2D climbMaterial;

    public PlayerData data;
    public PlayerInput InputHandler { get; private set; }
    // Список станів
    public PlayerIdleState IdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerMoveState WalkState { get; private set; }
    public PlayerCrouchState CrouchState { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Transform GroundCheck; 
    public bool IsGrounded { get; private set; }
    public Transform WallCheck;
    public bool IsTouchingWall { get; private set; }
    public PlayerWallClimbState WallClimbState { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        InputHandler = GetComponent<PlayerInput>();
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        WalkState = new PlayerMoveState(this, StateMachine);
        CrouchState = new PlayerCrouchState(this, StateMachine);
        WallClimbState = new PlayerWallClimbState(this, StateMachine);

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
        CheckIfTouchingWall();
    }
    private void CheckIfGrounded()
    {
        // Перевіряємо, чи є в заданому радіусі об'єкти з шаром "Земля"
        IsGrounded = Physics2D.OverlapCircle(GroundCheck.position, data.groundCheckRadius, data.whatIsGround);
    }
    public void SwapMaterial(PhysicsMaterial2D newMaterial)
    {
        if (Rigidbody.sharedMaterial != newMaterial)
        {
            Rigidbody.sharedMaterial = newMaterial;
        }
    }
    private void CheckIfTouchingWall()
    {
        // Пускаємо промінь вперед (враховуючи розворот спрайту через localScale.x)
        Vector2 castDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        IsTouchingWall = Physics2D.Raycast(WallCheck.position, castDirection, data.wallCheckDistance, data.whatIsWall);
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