using UnityEngine;

public class DisableImagesOnSize : MonoBehaviour
{
    public GameObject circleImage; // Reference to the circle image's GameObject
    public GameObject blackImage;  // Reference to the black image GameObject
    public GameObject pauseButton;
    public GameObject minimapButton;
    private int count;
    private RectTransform circleRect;

    void Start(){
        circleRect = circleImage.GetComponent<RectTransform>();
    }
    void Update()
    {   count++;
        if (count >= 1 && count < 10)
        {
            turnOn();
        }
        // Check if the circle's width and height have reached or exceeded 1000
        if (circleRect.rect.width >= 1000f && circleRect.rect.height >= 1000f)
        {
            // Disable the circle image and black image
            circleImage.SetActive(false);
            blackImage.SetActive(false);
            pauseButton.SetActive(true);
            minimapButton.SetActive(true);

            enabled = false;
        }
    }
    void turnOn(){
        circleImage.SetActive(false);
        circleImage.SetActive(true);
    }
}
