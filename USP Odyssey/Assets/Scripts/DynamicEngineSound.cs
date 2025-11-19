using UnityEngine;

public class DynamicEngineSound : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private Transform player;

    [Header("Distance Fading")]
    [SerializeField] private float maxVolume = 0.75f;
    [SerializeField] private float minVolume = 0.1f;
    [SerializeField] private float maxDistance = 60f;

    [Header("Engine Behaviour")]
    [SerializeField] private float maxEngineSpeed = 12f;
    [SerializeField] private float idlePitch = 0.9f;
    [SerializeField] private float drivePitch = 1.3f;
    [SerializeField] private float pitchResponse = 5f;
    [SerializeField] private bool randomizeLoopOffset = true;

    private Player playerScript;
    private Vector3 lastPosition;
    private float smoothedSpeed;

    private void Awake()
    {
        lastPosition = transform.position;
    }

    private void Start()
    {
        if (player == null)
        {
            Player found = FindFirstObjectByType<Player>();
            if (found != null)
            {
                player = found.transform;
            }
        }

        if (player != null)
        {
            playerScript = player.GetComponent<Player>();
        }

        if (engineAudioSource != null && randomizeLoopOffset && engineAudioSource.clip != null)
        {
            engineAudioSource.time = Random.Range(0f, engineAudioSource.clip.length);
        }
    }

    private void Update()
    {
        if (player == null || engineAudioSource == null)
        {
            return;
        }

        if (playerScript == null)
        {
            playerScript = player.GetComponent<Player>();
        }

        UpdatePitch();
        UpdateVolume();
    }

    private void UpdatePitch()
    {
        float frameSpeed = Vector3.Distance(transform.position, lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = transform.position;

        float normalizedSpeed = Mathf.Clamp01(frameSpeed / Mathf.Max(0.01f, maxEngineSpeed));
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, normalizedSpeed, Time.deltaTime * pitchResponse);

        float targetPitch = Mathf.Lerp(idlePitch, drivePitch, smoothedSpeed);
        engineAudioSource.pitch = targetPitch;
    }

    private void UpdateVolume()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance < maxDistance)
        {
            engineAudioSource.volume = Mathf.Lerp(maxVolume, minVolume, distance / maxDistance);
        }
        else
        {
            engineAudioSource.volume = minVolume;
        }
    }
}
