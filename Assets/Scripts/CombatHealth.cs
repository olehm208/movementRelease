
using UnityEngine;

public class CombatHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 6;

    public int CurrentHealth { get; private set; }

    private CharacterAnimationAPI animationAPI;

    public bool IsDead =>
        CurrentHealth <= 0 ||
        (animationAPI != null && animationAPI.IsDead);

    private void Awake()
    {
        CurrentHealth = maxHealth;
        animationAPI = GetComponentInChildren<CharacterAnimationAPI>();

        if (animationAPI == null)
            Debug.LogError("Character Animation API не знайдено", this);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"{name}: здоров'я {CurrentHealth}/{maxHealth}", this);

        if (CurrentHealth > 0)
        {
            animationAPI?.PlayHurt();
            return;
        }

        EnemyAnimationPresenter enemy = GetComponent<EnemyAnimationPresenter>();

        if (enemy != null)
        {
            enemy.PlayDeath();
            return;
        }

        animationAPI?.PlayDeath();

        PlayerController player = GetComponent<PlayerController>();

        if (player != null)
        {
            player.enabled = false;

            if (player.InputHandler != null)
                player.InputHandler.enabled = false;

            if (player.Rigidbody != null)
            {
                player.Rigidbody.linearVelocity = new Vector2(
                    0,
                    player.Rigidbody.linearVelocity.y
                );
            }
        }
    }
}