using UnityEngine;
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerData data;
    public PlayerInput InputHandler { get; private set; }
    // Список станів
    public PlayerIdleState IdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerMoveState WalkState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerDownJumpState DownJumpState { get; private set; }

    public Rigidbody2D Rigidbody { get; private set; }
    private Collider2D playerCollider;
    public Transform GroundCheck; 
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Rigidbody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        InputHandler = GetComponent<PlayerInput>();
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        WalkState = new PlayerMoveState(this, StateMachine);
        FallState = new PlayerFallState(this, StateMachine);
        DashState = new PlayerDashState(this, StateMachine);
        DownJumpState = new PlayerDownJumpState(this, StateMachine);
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
        bool groundUnderFeet = Physics2D.OverlapCircle(GroundCheck.position, data.groundCheckRadius, data.whatIsGround);

        // Колайдер гравця справді торкається землі, а не проходить крізь односторонню платформу
        bool isTouchingGround = playerCollider.IsTouchingLayers(data.whatIsGround);

        IsGrounded = groundUnderFeet && isTouchingGround;
    }
    private void OnDrawGizmos()
    {
        if (GroundCheck != null && data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheck.position, data.groundCheckRadius);
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}