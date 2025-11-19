using UnityEngine;

/// Adds a subtle pulse / spin so the minimap player marker feels alive.
/// Attach this to the RectTransform that represents the marker graphic.

[RequireComponent(typeof(RectTransform))]
public class MinimapMarkerAnimator : MonoBehaviour
{
    [Tooltip("Maximum percentage change in scale during the pulse (0.2 = ±20%).")]
    [SerializeField, Range(0f, 1f)] private float pulseAmplitude = 0.2f;
    [Tooltip("Speed of the pulse in cycles per second.")]
    [SerializeField, Min(0f)] private float pulseFrequency = 2f;
    [Tooltip("Optional continuous rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 45f;
    [Tooltip("If true, rotation oscillates back and forth instead of spinning 360°.")]
    [SerializeField] private bool pingPongRotation = true;
    private RectTransform rectTransform;
    private Vector3 baseScale;
    private float rotationAngle;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
    }
    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseFrequency) * pulseAmplitude;
        rectTransform.localScale = baseScale * pulse;
        if (Mathf.Approximately(rotationSpeed, 0f))
        {
            return;
        }
        if (pingPongRotation)
        {
            float angle = Mathf.Sin(Time.unscaledTime * pulseFrequency) * rotationSpeed;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            rotationAngle += rotationSpeed * Time.unscaledDeltaTime;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
    }
}