using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/// Handles showing a region name banner (e.g., "Downtown") whenever
/// RegionZone notifies it that the player entered a new area.

public class RegionDisplayManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("UI Text element that renders the region name.")]
    [SerializeField] private Text regionNameLabel;
    [Tooltip("CanvasGroup used to fade the banner in/out.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [Tooltip("Seconds it takes for the banner to fade in/out.")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.4f;
    [Tooltip("Seconds the banner stays fully visible before fading out.")]
    [SerializeField, Min(0f)] private float visibleDuration = 2f;

    [Header("Look & Feel")]
    [Tooltip("Optional curve to ease the fade animation.")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine activeRoutine;
    private string currentRegionName = string.Empty;
    private Graphic[] childGraphics;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        childGraphics = GetComponentsInChildren<Graphic>(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.ignoreParentGroups = true;
            canvasGroup.gameObject.SetActive(false);
        }

        if (regionNameLabel != null)
        {
            regionNameLabel.gameObject.SetActive(true);
        }
    }

    
    /// Displays the banner with the specified region name.
    
    public void ShowRegion(string regionName)
    {
        if (regionNameLabel == null || canvasGroup == null)
        {
            Debug.LogWarning("RegionDisplayManager is missing UI references.", this);
            return;
        }

        if (string.Equals(currentRegionName, regionName))
        {
            return;
        }

        currentRegionName = regionName;
        regionNameLabel.text = regionName;
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetChildRaycasts(true);

        activeRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return FadeCanvas(0f, 1f);

        yield return new WaitForSecondsRealtime(visibleDuration);
        yield return FadeCanvas(1f, 0f);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetChildRaycasts(false);
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
        currentRegionName = string.Empty;
    }

    private IEnumerator FadeCanvas(float from, float to)
    {
        if (Mathf.Approximately(fadeDuration, 0f))
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, fadeCurve.Evaluate(t));
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void SetChildRaycasts(bool enabled)
    {
        if (childGraphics == null)
        {
            childGraphics = GetComponentsInChildren<Graphic>(true);
        }

        for (int i = 0; i < childGraphics.Length; i++)
        {
            if (childGraphics[i] != null)
            {
                childGraphics[i].raycastTarget = enabled;
            }
        }
    }
}
