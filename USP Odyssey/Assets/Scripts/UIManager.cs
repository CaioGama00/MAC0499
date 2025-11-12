using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Collectible Panel")]
    public GameObject collectiblePanel;
    public Text collectibleNameUI;
    public Image collectibleImageUI;
    public Text collectibleDescriptionUI;

    [Header("Map Panel")]
    public GameObject mapPanel;

    private AudioSource uiAudioSource;
    private Player player;

    private void Awake()
    {
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        player = FindFirstObjectByType<Player>();
    }

    void Start()
    {
        if (collectiblePanel != null)
        {
            collectiblePanel.SetActive(false);
        }
    }

    public void ShowCollectiblePanel(Collectible collectible)
    {
        if (collectiblePanel == null) return;

        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }

        if (collectible.collectionSound != null)
        {
            uiAudioSource.PlayOneShot(collectible.collectionSound);
        }

        collectibleNameUI.text = collectible.collectibleName;
        collectibleImageUI.sprite = collectible.collectibleImage;
        collectibleImageUI.enabled = (collectible.collectibleImage != null);
        collectibleDescriptionUI.text = collectible.collectibleDescription;

        if (!string.IsNullOrEmpty(collectible.collectibleId) && collectible.collectibleId != "unique_collectible_id")
        {
            if (!player.HasCollectedCollectible(collectible.collectibleId))
            {
                player.RegisterCollectible(collectible.collectibleId);
            }
        }
        else
        {
            Debug.LogWarning($"Collectible '{collectible.name}' does not have a unique ID set. Its collected state will not be saved.", collectible);
        }

        collectible.gameObject.SetActive(false);
        collectiblePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideCollectiblePanel()
    {
        if (mapPanel != null)
        {
            mapPanel.SetActive(true);
        }
        collectiblePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
