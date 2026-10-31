using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 70f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 15f;

    private void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        CombatHealth health = player.GetComponent<CombatHealth>();

        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}