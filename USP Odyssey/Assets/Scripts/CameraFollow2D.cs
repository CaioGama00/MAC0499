using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public float smoothing = 5f;  // Speed of the camera smoothing
    public Vector3 offset;  // Offset of the camera from the player

    void Start()
    {
        // Initialize the camera's offset from the player
        if (player != null)
        {
            offset = transform.position - player.position;
        }
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            // Target position for the camera with the offset applied
            Vector3 targetPosition = player.position + offset;

            // Smoothly move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}
