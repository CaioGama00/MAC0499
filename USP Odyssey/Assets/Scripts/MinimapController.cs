using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RawImage minimapImage;
    public RawImage largeMapImage;
    [Tooltip("Root object that wraps the entire large map (optional).")]
    public GameObject largeMapRoot;
    public RectTransform largeMapPlayerMarker;
    public Button minimapButton;

    [Header("Map Settings")]
    public Transform playerTransform;
    [Tooltip("Renderer that covers the playable area. Leave null if you prefer to specify world bounds manually.")]
    public Renderer mapRenderer; // The Renderer of your game's ground/background sprite
    [Tooltip("Optional transforms marking the bottom-left and top-right corners of the world bounds.")]
    public Transform worldBottomLeft;
    public Transform worldTopRight;
    [Tooltip("Use XZ for 3D worlds, XY for 2D top-down maps.")]
    public bool useXZPlane = true;

    [Header("UI Elements to Disable")]
    public GameObject pauseButton;
    public GameObject movementJoystick;
    public GameObject collectiblesMenuButton;

    private bool isLargeMapActive = false;

    void Start()
    {
        GameObject root = ResolveLargeMapRoot();
        if (root != null)
        {
            root.SetActive(false);
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

        GameObject root = ResolveLargeMapRoot();
        if (root != null)
        {
            root.SetActive(isLargeMapActive);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(!isLargeMapActive);
        }

        if (movementJoystick != null)
        {
            movementJoystick.SetActive(!isLargeMapActive);
        }

        if (collectiblesMenuButton != null)
        {
            collectiblesMenuButton.SetActive(!isLargeMapActive);
        }
    }

    void UpdatePlayerMarker()
    {
        if (!TryGetNormalizedPlayerPosition(out Vector2 normalizedPosition))
        {
            return;
        }
        SetMarkerPosition(largeMapPlayerMarker, largeMapImage, normalizedPosition);
    }

    bool TryGetNormalizedPlayerPosition(out Vector2 normalizedPosition)
    {
        normalizedPosition = Vector2.zero;

        if (playerTransform == null)
        {
            return false;
        }

        Vector3 min, max;
        if (worldBottomLeft != null && worldTopRight != null)
        {
            min = worldBottomLeft.position;
            max = worldTopRight.position;
        }
        else if (mapRenderer != null)
        {
            Bounds mapBounds = mapRenderer.bounds;
            min = mapBounds.min;
            max = mapBounds.max;
        }
        else
        {
            return false;
        }

        float worldWidth = Mathf.Abs(max.x - min.x);
        float worldHeight = useXZPlane ? Mathf.Abs(max.z - min.z) : Mathf.Abs(max.y - min.y);

        if (worldWidth <= Mathf.Epsilon || worldHeight <= Mathf.Epsilon)
        {
            return false;
        }

        float normalizedX = Mathf.InverseLerp(min.x, max.x, playerTransform.position.x);
        float playerVertical = useXZPlane ? playerTransform.position.z : playerTransform.position.y;
        float normalizedY = useXZPlane
            ? Mathf.InverseLerp(min.z, max.z, playerVertical)
            : Mathf.InverseLerp(min.y, max.y, playerVertical);

        normalizedPosition = new Vector2(Mathf.Clamp01(normalizedX), Mathf.Clamp01(normalizedY));
        return true;
    }

    void SetMarkerPosition(RectTransform marker, RawImage targetImage, Vector2 normalizedPosition)
    {
        if (marker == null || targetImage == null)
        {
            return;
        }

        RectTransform targetRect = targetImage.rectTransform;
        Rect rect = targetRect.rect;

        Vector2 localPoint = new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, normalizedPosition.x),
            Mathf.Lerp(rect.yMin, rect.yMax, normalizedPosition.y)
        );

        Vector3 worldPoint = targetRect.TransformPoint(localPoint);
        if (marker.parent is RectTransform parentRect)
        {
            Vector3 parentSpace = parentRect.InverseTransformPoint(worldPoint);
            marker.anchoredPosition = new Vector2(parentSpace.x, parentSpace.y);
        }
        else
        {
            marker.position = worldPoint;
        }
    }

    private GameObject ResolveLargeMapRoot()
    {
        if (largeMapRoot != null)
        {
            return largeMapRoot;
        }

        return largeMapImage != null ? largeMapImage.gameObject : null;
    }
}
