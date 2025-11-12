using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public UIManager uiManager;
    [Tooltip("Any open panels here will be closed first when pressing Escape.")]
    public GameObject[] closeOnEscapePanels;

    [Header("Input")] 
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            // If a collectible panel is open, close it first
            if (uiManager != null && uiManager.collectiblePanel != null && uiManager.collectiblePanel.activeSelf)
            {
                uiManager.HideCollectiblePanel();
                return;
            }

            // Close any transient panels first (like enter car/bus prompts)
            if (closeOnEscapePanels != null)
            {
                bool closedAny = false;
                foreach (var panel in closeOnEscapePanels)
                {
                    if (panel != null && panel.activeSelf)
                    {
                        panel.SetActive(false);
                        closedAny = true;
                    }
                }
                if (closedAny) return; // Do not toggle pause if we just closed a panel
            }

            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
