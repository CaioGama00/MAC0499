using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public struct CollectibleInfo
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        public string Description;
        public Vector3 Position;
    }

    private static readonly Dictionary<string, CollectibleInfo> InfoById = new Dictionary<string, CollectibleInfo>();

    [Header("Collectible ID")]
    [Tooltip("A unique string to identify this collectible. E.g., 'Forest_Waterfall_Secret'. This MUST be unique for each collectible.")]
    public string collectibleId = "unique_collectible_id";

    [Header("Collectible Info")]
    public string collectibleName = "New Collectible";
    [Tooltip("Optional shorter label just for the menu. If empty, Collectible Name is used.")]
    public string collectibleMenuName = string.Empty;
    public Sprite collectibleImage;
    [Tooltip("Optional override for the menu icon. If empty the SpriteRenderer's sprite is used.")]
    public Sprite collectibleMenuIcon;
    [TextArea(3, 10)]
    public string collectibleDescription;

    [Header("Effects")]
    public AudioClip collectionSound; // Optional sound to play on collection

    private UIManager uiManager;

    private void Awake()
    {
        if (!ValidateCollectibleId())
        {
            return;
        }

        RegisterCollectibleInfo();

        if (PlayerPrefs.GetInt("Collectible_" + collectibleId, 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (ValidateCollectibleId())
        {
            RegisterCollectibleInfo();
        }
    }

    private void Start()
    {
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

    public static bool TryGetInfo(string collectibleId, out CollectibleInfo info)
    {
        return InfoById.TryGetValue(collectibleId, out info);
    }

    public static void GetAllInfos(List<CollectibleInfo> destination)
    {
        if (destination == null)
        {
            return;
        }

        destination.Clear();
        foreach (KeyValuePair<string, CollectibleInfo> entry in InfoById)
        {
            destination.Add(entry.Value);
        }
    }

    private bool ValidateCollectibleId()
    {
        if (string.IsNullOrEmpty(collectibleId) || collectibleId == "unique_collectible_id")
        {
            Debug.LogError($"Collectible '{gameObject.name}' has a missing or non-unique ID!", this);
            return false;
        }

        return true;
    }

    private void RegisterCollectibleInfo()
    {
        if (string.IsNullOrEmpty(collectibleId))
        {
            return;
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Sprite iconSprite = collectibleMenuIcon != null ? collectibleMenuIcon : renderer != null ? renderer.sprite : null;
        string displayName = !string.IsNullOrEmpty(collectibleMenuName) ? collectibleMenuName : collectibleName;

        CollectibleInfo info = new CollectibleInfo
        {
            Id = collectibleId,
            Name = displayName,
            Icon = iconSprite,
            Description = collectibleDescription,
            Position = transform.position
        };

        InfoById[collectibleId] = info;
    }
}
