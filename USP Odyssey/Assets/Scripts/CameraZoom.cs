using UnityEngine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private Slider zoomSlider; // Drag your slider here
    [SerializeField] private Camera mainCamera; // Drag your main camera here
    [SerializeField] private float minDistance = 50f; // Farthest distance (zoomed out)
    [SerializeField] private float maxDistance = 10f; // Closest distance (zoomed in)

    void Start()
    {
        // Set slider's saved value
        float savedZoom = PlayerPrefs.GetFloat("zoomCamera", 1f);
        zoomSlider.value = savedZoom; 

        // Add listener to detect slider changes
        zoomSlider.onValueChanged.AddListener(UpdateZoom);

        // Set initial zoom
        UpdateZoom(zoomSlider.value);
    }

    public void UpdateZoom(float sliderValue)
    {
        // Lerp (linear interpolation) between minDistance and maxDistance based on slider value
        float zoomDistance = Mathf.Lerp(minDistance, maxDistance, sliderValue);
        
        // Adjust orthographic size for orthographic cameras
        mainCamera.orthographicSize = zoomDistance;
        SaveZoom(sliderValue);
    }
    private void SaveZoom(float zoom)
    {
        PlayerPrefs.SetFloat("zoomCamera", zoom);
        PlayerPrefs.Save(); // Ensure changes are saved immediately
    }
}
