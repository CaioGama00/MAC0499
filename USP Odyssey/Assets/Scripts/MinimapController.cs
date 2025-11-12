using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RawImage minimapImage;
    public RawImage largeMapImage;
    public RectTransform playerMarker;
    public RectTransform largeMapPlayerMarker;
    public Button minimapButton;

    [Header("Map Settings")]
    public Transform playerTransform;
    public Renderer mapRenderer; // The Renderer of your game's ground/background sprite
    [Tooltip("Use XZ for 3D worlds, XY for 2D top-down maps.")]
    public bool useXZPlane = true;

    [Header("UI Elements to Disable")]
    public GameObject pauseButton;
    public GameObject movementJoystick;

    private bool isLargeMapActive = false;

    void Start()
    {
        if (largeMapImage != null)
        {
            largeMapImage.gameObject.SetActive(false);
        }

        if (minimapButton != null)
        {
            minimapButton.onClick.AddListener(ToggleLargeMap);
        }
    }

    void Update()
    {
        UpdatePlayerMarker();
    }

    void ToggleLargeMap()
    {
        isLargeMapActive = !isLargeMapActive;

        if (largeMapImage != null)
        {
            largeMapImage.gameObject.SetActive(isLargeMapActive);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(!isLargeMapActive);
        }

        if (movementJoystick != null)
        {
            movementJoystick.SetActive(!isLargeMapActive);
        }
    }

    void UpdatePlayerMarker()
    {
        if (!TryGetNormalizedPlayerPosition(out Vector2 normalizedPosition))
        {
            return;
        }

        SetMarkerPosition(playerMarker, minimapImage, normalizedPosition);
        SetMarkerPosition(largeMapPlayerMarker, largeMapImage, normalizedPosition);
    }

    bool TryGetNormalizedPlayerPosition(out Vector2 normalizedPosition)
    {
        normalizedPosition = Vector2.zero;

        if (playerTransform == null || mapRenderer == null)
        {
            return false;
        }

        Bounds mapBounds = mapRenderer.bounds;
        float width = mapBounds.size.x;
        float depthOrHeight = useXZPlane ? mapBounds.size.z : mapBounds.size.y;

        if (width <= Mathf.Epsilon || depthOrHeight <= Mathf.Epsilon)
        {
            return false;
        }

        float normalizedX = Mathf.InverseLerp(mapBounds.min.x, mapBounds.max.x, playerTransform.position.x);
        float playerVertical = useXZPlane ? playerTransform.position.z : playerTransform.position.y;
        float normalizedY = useXZPlane
            ? Mathf.InverseLerp(mapBounds.min.z, mapBounds.max.z, playerVertical)
            : Mathf.InverseLerp(mapBounds.min.y, mapBounds.max.y, playerVertical);

        normalizedPosition = new Vector2(Mathf.Clamp01(normalizedX), Mathf.Clamp01(normalizedY));
        return true;
    }

    void SetMarkerPosition(RectTransform marker, RawImage targetImage, Vector2 normalizedPosition)
    {
        if (marker == null || targetImage == null)
        {
            return;
        }

        Rect rect = targetImage.rectTransform.rect;

        // Anchor zero is at the center for typical minimap setups, so offset by half the rect.
        Vector2 anchoredPosition = new Vector2(
            (normalizedPosition.x - 0.5f) * rect.width,
            (normalizedPosition.y - 0.5f) * rect.height
        );

        marker.anchoredPosition = anchoredPosition;
    }
}
