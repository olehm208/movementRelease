using UnityEngine;

public class PlayerInput: MonoBehaviour
{
    public float normalizedInputX { get; set; }
    public float normalizedInputY { get; set; }
    public bool JumpInput { get; set; }
    public bool DashInput { get; set; }

    void Update()
    {
        normalizedInputX = Input.GetAxisRaw("Horizontal");
        normalizedInputY = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            JumpInput = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            DashInput = true;
        }
    }
    
    public void UseJumpInput()
    {
        JumpInput = false;
    }
    public void UseDashInput()
    {
        DashInput = false;
    }
}