using System;
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }
    public event Action<PlayerState, PlayerState> StateChanged;

    public void Initialize(PlayerState startingState)
    {
        PlayerState previous = CurrentState;

        CurrentState = startingState;
        CurrentState.Enter();

        StateChanged?.Invoke(previous, CurrentState);
    }

    public void ChangeState(PlayerState newState)
    {
        PlayerState previous = CurrentState;

        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();

        StateChanged?.Invoke(previous, CurrentState);
    }
}