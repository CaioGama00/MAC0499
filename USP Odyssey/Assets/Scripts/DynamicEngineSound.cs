using UnityEngine;

public class DynamicEngineSound : MonoBehaviour
{
    public AudioSource engineAudioSource; // Reference to the engine's AudioSource.
    public Transform player; // Reference to the player GameObject.
    public float maxVolume; // The maximum volume of the engine sound.
    public float minVolume; // The minimum volume of the engine sound.
    public float maxDistance; // The maximum distance where the sound is audible.
    private Player playerScript; // Reference to the player script.


    void Start()
    {
        playerScript = player.GetComponent<Player>();
    }
    void Update()
    {   
        if (!playerScript.InBus() && player != null && engineAudioSource != null)
        {
            // Calculate the distance between the player and the bus.
            float distance = Vector3.Distance(player.position, transform.position);

            // Adjust the volume based on the distance.
            if (distance < maxDistance)
            {
                float volume = Mathf.Lerp(maxVolume, minVolume, distance / maxDistance);
                engineAudioSource.volume = volume;
            }
            else
            {
                engineAudioSource.volume = minVolume; // Set to minimum volume if out of range.
            }
        }
    }
}
