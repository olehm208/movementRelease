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
    [Tooltip("Опір в повітрі, впливає на те чи можна рухатись туди сюди")]
    public float airControl = 2f;
    [Tooltip("Кількість можливих стрибків (наприклад, 2 для подвійного стрибка)")]
    public int amountOfJumps = 1;
    [Tooltip("Множник гравітації при падінні (щоб падіння було швидшим за стрибок)")]
    public float fallGravityMultiplier = 2.5f;
    [Header("Присідання (Crouch)")]
    [Tooltip("Множник швидкості присідання")]
    public float crouchSpeedMultiplier = .5f;
    [Tooltip("Швидкість різкого падіння")]
    public float fastFallSpeed = -25f;
    [Header("Стіни (Wall Climb)")]
    public float wallClimbVelocity = 3f;
    public float wallCheckDistance = 0.5f;
    // Сила відштовхування від стіни (X - по горизонталі, Y - по вертикалі)
    public Vector2 wallJumpForce = new Vector2(8f, 10f); 
    // Шари, по яких можна лазити (можна використовувати той самий шар, що й whatIsGround)
    public LayerMask whatIsWall;
    [Header("Перевірки оточення (Physics Checks)")]
    [Tooltip("Радіус сфери для перевірки торкання землі")]
    public float groundCheckRadius = 0.3f;
    [Tooltip("Шари, які вважаються землею")]
    public LayerMask whatIsGround;

}