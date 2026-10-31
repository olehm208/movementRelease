using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WizardBoss : MonoBehaviour
{
    [Header("Магія")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Vector2 spawnOffset = new Vector2(-1f, 0f);
    [SerializeField] private float attackInterval = 3f;

    private Animator animator;
    private CombatHealth health;
    private float nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<CombatHealth>();
    }

    private void Start()
    {
        nextAttackTime = Time.time + attackInterval;
    }

    private void Update()
    {
        if (health != null && health.IsDead)
            return;

        PlayerController player = PlayerController.Instance;

        if (player == null)
            return;

        CombatHealth playerHealth = player.GetComponent<CombatHealth>();

        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Time.time >= nextAttackTime)
        {
            animator.SetTrigger("Attack");
            nextAttackTime = Time.time + attackInterval;
        }
    }

    // Викликається подією в анімації атаки
    public void FireProjectile()
    {
        if (health != null && health.IsDead)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogError("Не призначено Prefab снаряда!", this);
            return;
        }

        Vector3 position = transform.position + (Vector3)spawnOffset;
        Instantiate(projectilePrefab, position, Quaternion.identity);
    }
}