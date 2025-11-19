using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectiblesMenuUI : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform entriesParent;
    [SerializeField] private CollectibleMenuEntry entryPrefab;
    [Header("Progress UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressLabel;

    [Header("Gameplay References")]
    [SerializeField] private Player player;
    [SerializeField] private bool pauseTimeWhileOpen = true;
    [SerializeField] private GameObject miniMapRoot;

    private readonly List<CollectibleMenuEntry> spawnedEntries = new List<CollectibleMenuEntry>();
    private readonly List<Collectible.CollectibleInfo> collectibleInfos = new List<Collectible.CollectibleInfo>();
    private bool menuOpen;
    private float previousTimeScale = 1f;
    private bool minimapWasActive;

    public void ToggleMenu()
    {
        if (menuOpen)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    public void ShowMenu()
    {
        if (menuPanel == null || entryPrefab == null || entriesParent == null || player == null)
        {
            Debug.LogWarning($"CollectiblesMenuUI is missing references. menuPanel:{menuPanel != null} entriesParent:{entriesParent != null} entryPrefab:{entryPrefab != null} player:{player != null}", this);
            return;
        }

        ClearEntries();
        Collectible.GetAllInfos(collectibleInfos);
        Debug.Log($"CollectiblesMenuUI found {collectibleInfos.Count} collectibles.", this);
        if (collectibleInfos.Count == 0)
        {
            Debug.LogWarning("CollectiblesMenuUI could not find any registered collectibles.", this);
            UpdateProgressElements(0, 0);
        }
        else
        {
            collectibleInfos.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            int collectedCount = 0;
            for (int i = 0; i < collectibleInfos.Count; i++)
            {
                CollectibleMenuEntry entry = Instantiate(entryPrefab, entriesParent, false);
                bool unlocked = player != null && player.HasCollectedCollectible(collectibleInfos[i].Id);
                if (unlocked)
                {
                    collectedCount++;
                }
                bool allowedToInteract = unlocked && (player == null || !player.InVehicle());
                Debug.Log($"Spawning entry for {collectibleInfos[i].Id}. Unlocked: {unlocked}");
                entry.Initialize(collectibleInfos[i], unlocked, allowedToInteract, OnEntrySelected);
                spawnedEntries.Add(entry);
            }

            UpdateProgressElements(collectedCount, collectibleInfos.Count);
        }

        menuPanel.SetActive(true);

        if (miniMapRoot != null)
        {
            minimapWasActive = miniMapRoot.activeSelf;
            miniMapRoot.SetActive(false);
        }

        if (pauseTimeWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        menuOpen = true;
    }

    public void HideMenu()
    {
        if (!menuOpen)
        {
            return;
        }

        if (pauseTimeWhileOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        if (miniMapRoot != null)
        {
            miniMapRoot.SetActive(minimapWasActive);
        }

        ClearEntries();
        menuOpen = false;
    }

    private void UpdateProgressElements(int collected, int total)
    {
        if (progressBar != null)
        {
            progressBar.maxValue = Mathf.Max(1, total);
            progressBar.value = total == 0 ? 0 : collected;
        }

        if (progressLabel != null)
        {
            progressLabel.text = total > 0
                ? $"Coletados: {collected}/{total}"
                : "Coletados: 0/0";
        }
    }

    private void ClearEntries()
    {
        for (int i = spawnedEntries.Count - 1; i >= 0; i--)
        {
            if (spawnedEntries[i] != null)
            {
                Destroy(spawnedEntries[i].gameObject);
            }
        }

        spawnedEntries.Clear();
    }

    private void OnEntrySelected(Collectible.CollectibleInfo info)
    {
        HideMenu();
        if (player != null)
        {
            player.TeleportTo(info.Position);
        }
    }
}
