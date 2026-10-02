using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerData data;

    // Список станів
    public PlayerIdleState IdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    public Rigidbody Rigidbody { get; private set; }
    public Transform GroundCheck; 
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
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
    }
    private void CheckIfGrounded()
    {
        // Перевіряємо, чи є в заданому радіусі об'єкти з шаром "Земля"
        IsGrounded = Physics.CheckSphere(GroundCheck.position, data.groundCheckRadius, data.whatIsGround);
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