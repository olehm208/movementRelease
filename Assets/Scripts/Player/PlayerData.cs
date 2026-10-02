using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Рух (Movement)")]
    [Tooltip("Базова швидкість переміщення гравця")]
    public float movementVelocity = 8f;
[Header("Стрибок (Jump)")]
    [Tooltip("Сила, з якою гравець відштовхується від землі")]
    public float jumpForce = 12f;
    [Tooltip("Кількість можливих стрибків (наприклад, 2 для подвійного стрибка)")]
    public int amountOfJumps = 1;
    [Tooltip("Множник гравітації при падінні (щоб падіння було швидшим за стрибок)")]
    public float fallGravityMultiplier = 2.5f;
    [Header("Перевірки оточення (Physics Checks)")]
    [Tooltip("Радіус сфери для перевірки торкання землі")]
    public float groundCheckRadius = 0.3f;
    [Tooltip("Шари, які вважаються землею")]
    public LayerMask whatIsGround;

}