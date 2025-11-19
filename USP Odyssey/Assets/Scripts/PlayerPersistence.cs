using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    private const float MinimumAutoSaveInterval = 5f;

    [SerializeField] private int startingLives = 3;
    [SerializeField] private string defaultPlayerName = "Default Name";

    private PlayerMovement movement;
    private float autoSaveIntervalSeconds = 30f;
    private Coroutine autoSaveCoroutine;
    private bool saveQueuedDuringWrite;
    private bool isSaving;

    private List<string> collectedCollectibleIds = new List<string>();
    private int coins;
    private int lives;
    private string playerName;

    public IReadOnlyList<string> CollectedCollectibleIds => collectedCollectibleIds;

    public void Configure(PlayerMovement playerMovement, float autoSaveInterval)
    {
        movement = playerMovement;
        AutoSaveIntervalSeconds = autoSaveInterval;
    }

    public float AutoSaveIntervalSeconds
    {
        get => autoSaveIntervalSeconds;
        set
        {
            autoSaveIntervalSeconds = Mathf.Max(0f, value);
            RestartAutoSave();
        }
    }

    private void OnEnable()
    {
        RestartAutoSave();
    }

    private void OnDisable()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }

        RequestSave();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            RequestSave();
        }
    }

    public void LoadGame()
    {
        PlayerData data = SaveManager.LoadGame();
        if (data != null)
        {
            movement?.WarpTo(data.position);
            coins = data.coins;
            lives = data.lives;
            playerName = data.playerName;
            collectedCollectibleIds = data.collectedCollectibleIds != null
                ? new List<string>(data.collectedCollectibleIds)
                : new List<string>();
            Debug.Log("Game Loaded!");
            ImportLegacyCollectiblesFromPrefs();
            ApplyCollectedCollectibles();
        }
        else
        {
            coins = 0;
            lives = startingLives;
            playerName = defaultPlayerName;
            collectedCollectibleIds = new List<string>();
            Debug.Log("No save file found. Starting new game.");
            ImportLegacyCollectiblesFromPrefs();
            ApplyCollectedCollectibles();
        }
    }

    public void RequestSave()
    {
        _ = SaveAsync();
    }

    public bool HasCollectedCollectible(string collectibleId)
    {
        return collectedCollectibleIds.Contains(collectibleId);
    }

    public void RegisterCollectible(string collectibleId)
    {
        if (string.IsNullOrEmpty(collectibleId))
        {
            return;
        }

        if (!collectedCollectibleIds.Contains(collectibleId))
        {
            collectedCollectibleIds.Add(collectibleId);
            RequestSave();
        }
    }

    private async Task SaveAsync()
    {
        if (isSaving)
        {
            saveQueuedDuringWrite = true;
            return;
        }

        isSaving = true;
        do
        {
            saveQueuedDuringWrite = false;
            PlayerData data = CaptureSnapshot();
            bool success = await SaveManager.SaveGameAsync(data);
            if (!success)
            {
                Debug.LogWarning("Save attempt failed.");
            }
        }
        while (saveQueuedDuringWrite);
        isSaving = false;
    }

    private PlayerData CaptureSnapshot()
    {
        List<string> collectibleSnapshot = collectedCollectibleIds != null
            ? new List<string>(collectedCollectibleIds)
            : new List<string>();

        return new PlayerData
        {
            position = movement != null ? movement.GetPosition() : transform.position,
            coins = coins,
            lives = lives,
            playerName = playerName,
            collectedCollectibleIds = collectibleSnapshot
        };
    }

    private void ApplyCollectedCollectibles()
    {
        Collectible[] allCollectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible != null && collectedCollectibleIds.Contains(collectible.collectibleId))
            {
                collectible.gameObject.SetActive(false);
            }
        }
    }

    private void ImportLegacyCollectiblesFromPrefs()
    {
        Collectible[] allCollectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        bool addedAny = false;

        foreach (Collectible collectible in allCollectibles)
        {
            if (collectible == null || string.IsNullOrEmpty(collectible.collectibleId))
            {
                continue;
            }

            string legacyKey = "Collectible_" + collectible.collectibleId;
            if (PlayerPrefs.GetInt(legacyKey, 0) == 1 && !collectedCollectibleIds.Contains(collectible.collectibleId))
            {
                collectedCollectibleIds.Add(collectible.collectibleId);
                addedAny = true;
            }
        }

        if (addedAny)
        {
            RequestSave();
        }
    }

    private void RestartAutoSave()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }

        if (autoSaveIntervalSeconds > 0f)
        {
            autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            float waitTime = Mathf.Max(MinimumAutoSaveInterval, autoSaveIntervalSeconds);
            yield return new WaitForSeconds(waitTime);
            RequestSave();
        }
    }
}
