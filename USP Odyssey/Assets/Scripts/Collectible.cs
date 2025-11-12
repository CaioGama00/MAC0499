using UnityEngine;

public class Collectible : MonoBehaviour
{   
    [Header("Collectible ID")]
    [Tooltip("A unique string to identify this collectible. E.g., 'Forest_Waterfall_Secret'. This MUST be unique for each collectible.")]
    public string collectibleId = "unique_collectible_id";

    [Header("Collectible Info")]
    public string collectibleName = "New Collectible";
    public Sprite collectibleImage;
    [TextArea(3, 10)]
    public string collectibleDescription;

    [Header("Effects")]
    public AudioClip collectionSound; // Optional sound to play on collection

    private UIManager uiManager;

    private void Awake()
    {
        if (string.IsNullOrEmpty(collectibleId) || collectibleId == "unique_collectible_id")
        {
            Debug.LogError($"Collectible '{gameObject.name}' has a missing or non-unique ID!", this);
            return;
        }

        // Check if this collectible has already been collected
        if (PlayerPrefs.GetInt("Collectible_" + collectibleId, 0) == 1)
        {
            gameObject.SetActive(false); // Hide it if already collected
        }
    }

    private void Start()
    {
        // Find the UIManager in the scene.
        // This assumes you have one UIManager.
        uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found in the scene. Please add a UIManager component to a GameObject.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            uiManager.ShowCollectiblePanel(this);
        }
    }
}
