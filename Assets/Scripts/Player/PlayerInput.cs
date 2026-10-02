using UnityEngine;

public class PlayerInput: MonoBehaviour
{
    public float normalizedInputX { get; private set; }
    public float normalizedInputY { get; private set; }
    public bool JumpInput { get; private set; }
    public bool isCrouching { get; private set; }

void Update()
    {
        normalizedInputX = Input.GetAxisRaw("Horizontal");
        normalizedInputY = Input.GetAxisRaw("Vertical");
        
        // Якщо гравець тисне стрілку вниз або 'S' (значення Y стає від'ємним)
        isCrouching = normalizedInputY < -0.5f;

        // Зчитуємо стрибок ТІЛЬКИ в момент натискання кнопки
        if (Input.GetButtonDown("Jump"))
        {
            JumpInput = true;
        }
    }
    
    public void UseJumpInput()
    {
        JumpInput = false;
    }
}