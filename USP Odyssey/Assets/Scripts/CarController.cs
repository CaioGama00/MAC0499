using UnityEngine;

public class CarController : MonoBehaviour
{
    public MovementJoystick movementJoystick;  // Reference to joystick (can be null on PC)
    private CarMovement carMovement;
    private bool isDriving = false;

    public void StartDriving(CarMovement carMovement)
    {
        this.carMovement = carMovement;
        isDriving = true;
        carMovement.enabled = true;
    }

    public void StopDriving()
    {
        isDriving = false;
        carMovement.enabled = false;
    }

    private void Update()
    {
        if (!isDriving || carMovement == null)
            return;

        Vector2 inputVector = Vector2.zero;

        // 🔹 1. Touch or joystick input (for phones)
        if (movementJoystick != null && 
            (movementJoystick.joystickVec.x != 0 || movementJoystick.joystickVec.y != 0))
        {
            inputVector = new Vector2(movementJoystick.joystickVec.x, movementJoystick.joystickVec.y);
        }
        else
        {
            // 🔹 2. Keyboard input (for PC)
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
            float vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down
            inputVector = new Vector2(horizontal, vertical);
        }

        // Normalize to avoid faster diagonal movement
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);

        // Send input to CarMovement
        carMovement.SetInputVector(inputVector);
    }
}
