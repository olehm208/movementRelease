using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatHealth))]
public class CombatAttack : MonoBehaviour
{
    [Header("Удар")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitOffset = 0.8f;
    [SerializeField] private float hitRadius = 1f;

    [Header("Атака ворога")]
    [SerializeField] private float enemyAttackRange = 2f;
    [SerializeField] private float enemyAttackCooldown = 2.5f;

    private CharacterAnimationAPI animationAPI;
    private CombatHealth health;
    private EnemyController enemy;
    private float nextEnemyAttack;

    private void Awake()
    {
        health = GetComponent<CombatHealth>();
        enemy = GetComponent<EnemyController>();
        animationAPI = GetComponentInChildren<CharacterAnimationAPI>();

        if (animationAPI == null)
            Debug.LogError("Character Animation API не знайдено", this);
    }

    private void OnEnable()
    {
        if (animationAPI != null)
            animationAPI.InteractionFrame += DealHit;
    }

    private void OnDisable()
    {
        if (animationAPI != null)
            animationAPI.InteractionFrame -= DealHit;
    }

    private void Update()
    {
        // Гравець атакує через наявну клавішу E.
        // Ця частина запускає автоматичну атаку тільки для ворога.
        if (enemy == null || animationAPI == null || health.IsDead)
            return;

        PlayerController player = PlayerController.Instance;

        if (player == null || !enemy.IsChasing)
            return;

        CombatHealth playerHealth = player.GetComponent<CombatHealth>();

        if (playerHealth != null && playerHealth.IsDead)
            return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance > enemyAttackRange || Time.time < nextEnemyAttack)
            return;

        // Повертаємо ворога до гравця перед атакою.
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);

        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }

        animationAPI.PlayInteraction();
        nextEnemyAttack = Time.time + enemyAttackCooldown;
    }

    private void DealHit()
    {
        if (health.IsDead)
            return;

        float direction = Mathf.Sign(transform.lossyScale.x);
        Vector2 hitCenter = (Vector2)transform.position
                            + Vector2.right * direction * hitOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, hitRadius);
        HashSet<CombatHealth> damagedTargets = new HashSet<CombatHealth>();

        foreach (Collider2D hit in hits)
        {
            CombatHealth target = hit.GetComponentInParent<CombatHealth>();

            if (target == null || target == health || target.IsDead)
                continue;

            // Ворог б'є гравця, а гравець — ворога.
            if (enemy != null && target.GetComponent<PlayerController>() == null)
                continue;

            if (enemy == null && target.GetComponent<PlayerController>() != null)
                continue;

            float targetSide = (target.transform.position.x - transform.position.x) * direction;

            if (targetSide < -0.05f)
                continue;

            // Один маркер анімації завдає цілі шкоди лише один раз.
            if (damagedTargets.Add(target))
                target.TakeDamage(damage);
        }
    }
}