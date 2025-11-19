using UnityEngine;


/// Makes a GameObject gently bob up and down in place.

public class ItemBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [Tooltip("How fast the item bobs up and down.")]
    public float frequency = 2f;

    [Tooltip("How high the item bobs from its starting point.")]
    public float amplitude = 0.2f;

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position of the object.
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave for smooth bobbing.
        transform.position = startPosition + new Vector3(0, Mathf.Sin(Time.time * frequency) * amplitude, 0);
    }
}