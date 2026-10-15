
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationPresenter : MonoBehaviour
{
    [SerializeField] private CharacterAnimationAPI animationAPI;

    private PlayerController player;
    private PlayerState lastState;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        if (animationAPI == null)
            animationAPI = GetComponentInChildren<CharacterAnimationAPI>(true);
    }

    private void Start()
    {
        if (animationAPI == null)
        {
            Debug.LogError("CharacterAnimationAPI not found!", this);
            enabled = false;
            return;
        }

        player.StateMachine.StateChanged += OnStateChanged;

        ApplyState(player.StateMachine.CurrentState, false);
    }

    private void Update()
    {
        if (player.StateMachine.CurrentState != lastState)
            ApplyState(player.StateMachine.CurrentState, false);
    }

    private void OnDestroy()
    {
        if (player != null && player.StateMachine != null)
            player.StateMachine.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(PlayerState previous, PlayerState current)
    {
        if (!isActiveAndEnabled)
            return;

        bool restart = ReferenceEquals(previous, current);

        ApplyState(current, restart);
    }

    private void ApplyState(PlayerState state, bool restart)
    {
        if (state == null || animationAPI == null)
            return;

        lastState = state;

        CharacterMotionState motion = CharacterMotionState.Idle;

        if (state == player.WalkState)
            motion = CharacterMotionState.Move;

        else if (state == player.JumpState)
            motion = CharacterMotionState.Jump;

        else if (state == player.FallState)
            motion = CharacterMotionState.Fall;

        else if (state == player.DashState)
            motion = CharacterMotionState.Dash;

        else if (state == player.DownJumpState)
            motion = CharacterMotionState.Drop;

        animationAPI.SetMotion(motion, restart);
    }

    public void PlayHurt()
    {
        animationAPI?.PlayHurt();
    }

    public void PlayInteraction()
    {
        animationAPI?.PlayInteraction();
    }

    public void PlayDeath()
    {
        animationAPI?.PlayDeath();
    }
}
