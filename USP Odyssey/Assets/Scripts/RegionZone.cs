using System.Collections.Generic;
using UnityEngine;


/// Marks a rectangular/shape area as a named region. When the player enters,
/// the RegionDisplayManager is asked to show the region name.

[RequireComponent(typeof(Collider2D))]
public class RegionZone : MonoBehaviour
{
    private static readonly List<RegionZone> ActiveZones = new List<RegionZone>();
    private static bool regionsEnabled;

    [Tooltip("Shown when the player enters this zone.")]
    [SerializeField] private string regionName = "New Region";

    [Tooltip("Optional override. If left empty the zone will auto-locate the RegionDisplayManager in the scene.")]
    [SerializeField] private RegionDisplayManager displayManager;

    private Collider2D zoneCollider;
    private Collider2D playerCollider;
    private PlayerVehicleController vehicleController;
    private int lastDisplayFrame = -1;

    private void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();

        if (displayManager == null)
        {
            displayManager = FindFirstObjectByType<RegionDisplayManager>();
        }

        if (vehicleController == null)
        {
            vehicleController = FindFirstObjectByType<PlayerVehicleController>();
        }

        if (zoneCollider != null && !zoneCollider.isTrigger)
        {
            zoneCollider.isTrigger = true;
        }

        CachePlayerCollider();
    }

    private void OnEnable()
    {
        if (!ActiveZones.Contains(this))
        {
            ActiveZones.Add(this);
        }

        if (regionsEnabled)
        {
            TryShowRegionIfPlayerInside();
        }
    }

    private void OnDisable()
    {
        ActiveZones.Remove(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!regionsEnabled || displayManager == null || !IsPlayerControlledCollider(other))
        {
            return;
        }

        ShowRegion();
    }

    private void TryShowRegionIfPlayerInside()
    {
        if (!regionsEnabled || displayManager == null || zoneCollider == null)
        {
            return;
        }

        CachePlayerCollider();

        if (vehicleController != null && vehicleController.IsAnyControlledColliderTouching(zoneCollider))
        {
            ShowRegion();
            return;
        }

        if (playerCollider != null && zoneCollider.IsTouching(playerCollider))
        {
            ShowRegion();
        }
    }

    private void CachePlayerCollider()
    {
        if (playerCollider == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerCollider = player.GetComponent<Collider2D>();
                if (vehicleController == null)
                {
                    vehicleController = player.GetComponent<PlayerVehicleController>();
                }
            }
        }
    }

    private bool IsPlayerControlledCollider(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        if (vehicleController != null && vehicleController.ControlsCollider(other))
        {
            return true;
        }

        if (playerCollider != null && other == playerCollider)
        {
            return true;
        }

        if (other.GetComponent<Player>() != null || other.GetComponent<PlayerMovement>() != null)
        {
            return true;
        }

        return false;
    }

    private void ShowRegion()
    {
        if (displayManager == null)
        {
            return;
        }

        if (lastDisplayFrame == Time.frameCount)
        {
            return;
        }

        lastDisplayFrame = Time.frameCount;
        displayManager.ShowRegion(regionName);
    }

    public static void EnableRegions()
    {
        if (regionsEnabled)
        {
            return;
        }

        regionsEnabled = true;
        for (int i = 0; i < ActiveZones.Count; i++)
        {
            ActiveZones[i]?.TryShowRegionIfPlayerInside();
        }
    }
}
