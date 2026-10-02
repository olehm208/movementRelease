using UnityEngine;

public class PlayerState
{
    // контроллер гравця
    protected PlayerController player;
    // стан-машина
    protected PlayerStateMachine stateMachine;

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }
    // всі можливі стани
    public virtual void Enter() { } //перехід в стан
    public virtual void HandleInput() { } //робота з інпутом
    public virtual void LogicUpdate() { } //update
    public virtual void PhysicsUpdate() { } //FixedUpdate
    public virtual void Exit() { } //вихід з стану
}
