
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class AnimationDemoHotkeys : MonoBehaviour
{
    [SerializeField] private PlayerAnimationPresenter animations;

    private PlayerController player;
    private bool isDead;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        if (animations == null)
            animations = GetComponent<PlayerAnimationPresenter>();
    }

    private void Update()
    {
        if (isDead || animations == null)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            animations.PlayInteraction();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            animations.PlayHurt();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            isDead = true;

            animations.PlayDeath();

            player.enabled = false;
            player.InputHandler.enabled = false;
            player.Rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
