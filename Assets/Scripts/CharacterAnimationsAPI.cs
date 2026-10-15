
using System;
using UnityEngine;
using UnityEngine.Events;

public enum CharacterMotionState
{
    Idle = 0,
    Move = 1,
    Jump = 2,
    Fall = 3,
    Dash = 4,
    Drop = 5,
    Chase = 6
}

[RequireComponent(typeof(Animator))]
public class CharacterAnimationAPI : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animation Events")]
    [SerializeField] private UnityEvent onInteraction;
    [SerializeField] private UnityEvent onHurt;

    public event Action InteractionFrame;
    public event Action HurtFrame;

    public CharacterMotionState CurrentMotion { get; private set; }
    public bool IsDead { get; private set; }

    private static readonly int Motion = Animator.StringToHash("Motion");
    private static readonly int Replay = Animator.StringToHash("Replay");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Interact = Animator.StringToHash("Interact");
    private static readonly int Dead = Animator.StringToHash("Dead");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.applyRootMotion = false;
    }

    public void SetMotion(CharacterMotionState state, bool restart = false)
    {
        if (IsDead || animator == null)
            return;

        if (CurrentMotion == state && !restart)
            return;

        CurrentMotion = state;
        animator.SetInteger(Motion, (int)state);

        if (restart)
        {
            animator.ResetTrigger(Replay);
            animator.SetTrigger(Replay);
        }
    }

    public void PlayHurt()
    {
        if (IsDead) return;

        animator.SetTrigger(Hurt);
    }

    public void PlayInteraction()
    {
        if (IsDead) return;

        animator.SetTrigger(Interact);
    }

    public void PlayDeath()
    {
        if (IsDead) return;

        IsDead = true;

        animator.ResetTrigger(Hurt);
        animator.ResetTrigger(Interact);
        animator.ResetTrigger(Replay);

        animator.SetBool(Dead, true);
    }

    // Викликається через Animation Event
    public void AnimationEvent_Interact()
    {
        Debug.Log("[Animation Event] Interaction!", this);

        onInteraction?.Invoke();
        InteractionFrame?.Invoke();
    }

    public void AnimationEvent_Hurt()
    {
        Debug.Log("[Animation Event] Hurt!", this);

        onHurt?.Invoke();
        HurtFrame?.Invoke();
    }
}
