using UnityEngine;

public class PlayerInput: MonoBehaviour
{
    public float normalizedInputX { get; set; }
    public bool JumpInput { get; set; }

    void Update()
    {
        normalizedInputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetAxisRaw("Jump") > 0)
        {
            JumpInput = true;
        }
    }
    
    public void UseJumpInput()
    {
        JumpInput = false;
    }
}