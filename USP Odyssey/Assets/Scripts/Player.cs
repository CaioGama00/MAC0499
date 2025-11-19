using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerVehicleController))]
[RequireComponent(typeof(PlayerPersistence))]
public class Player : MonoBehaviour
{
    private const string MusicVolumePrefKey = "musicVolume";
    private const string ZoomPrefKey = "zoomCamera";
    private const float MaxCameraSize = 50f;
    private const float MinCameraSize = 10f;

    [SerializeField] private MovementJoystick movementJoystick;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource panelSound;
    [SerializeField] private GameObject enterCarPanel;
    [SerializeField] private GameObject enterBusPanel;
    [SerializeField] private GameObject movementJoystickPanel;
    [SerializeField] private GameObject inBusPanel;
    [SerializeField] private GameObject exitBusPanel;
    [SerializeField] private GameObject[] inBusPanelElementsToDisable;
    [SerializeField] private CameraFollow2D mainCamera;
    [FormerlySerializedAs("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject[] invisibleWalls;
    [SerializeField] private float exitCooldownDuration = 1f;
    [SerializeField] private float autoSaveIntervalSeconds = 30f;

    private PlayerMovement movement;
    private PlayerVehicleController vehicleController;
    private PlayerPersistence persistence;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<PlayerMovement>();
        }

        vehicleController = GetComponent<PlayerVehicleController>();
        if (vehicleController == null)
        {
            vehicleController = gameObject.AddComponent<PlayerVehicleController>();
        }

        persistence = GetComponent<PlayerPersistence>();
        if (persistence == null)
        {
            persistence = gameObject.AddComponent<PlayerPersistence>();
        }

        movement.Configure(movementJoystick, playerSpeed);
        vehicleController.Configure(
            movement,
            movementJoystickPanel,
            inBusPanel,
            exitBusPanel,
            enterCarPanel,
            enterBusPanel,
            miniMap,
            invisibleWalls,
            buttonClickSound,
            panelSound,
            mainCamera,
            playerCamera,
            exitCooldownDuration,
            inBusPanelElementsToDisable);
        persistence.Configure(movement, autoSaveIntervalSeconds);
    }

    private void Start()
    {
        persistence.LoadGame();
        AudioListener.volume = ReadNormalizedPreference(MusicVolumePrefKey, 1f);

        if (playerCamera != null)
        {
            float zoomPreference = ReadNormalizedPreference(ZoomPrefKey, 1f);
            playerCamera.orthographicSize = Mathf.Lerp(MaxCameraSize, MinCameraSize, zoomPreference);
        }

        if (mainCamera != null)
        {
            mainCamera.SetTarget(transform, false);
        }
    }

    public void SaveGame()
    {
        persistence.RequestSave();
    }

    public void LoadGame()
    {
        persistence.LoadGame();
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        movement.SetSpawnPoint(newSpawnPoint);
    }

    public void TeleportTo(Vector3 destination)
    {
        movement.WarpTo(destination);
        if (mainCamera != null)
        {
            mainCamera.SetTarget(transform, false);
        }
    }

    public bool InBus()
    {
        return vehicleController.IsInBus;
    }

    public bool InVehicle()
    {
        return vehicleController.IsInBus || vehicleController.IsInCar;
    }

    public void ButtonPressed()
    {
        vehicleController.ButtonPressed();
    }

    public void OnYesButtonClicked()
    {
        vehicleController.OnYesButtonClicked();
    }

    public void OnNoButtonClicked()
    {
        vehicleController.OnNoButtonClicked();
    }

    public void OnYesBusButtonClicked()
    {
        vehicleController.OnYesBusButtonClicked();
    }

    public void OnNoBusButtonClicked()
    {
        vehicleController.OnNoBusButtonClicked();
    }

    public void OnAcceleratePress()
    {
        vehicleController.OnAcceleratePress();
    }

    public void OnBrakePress()
    {
        vehicleController.OnBrakePress();
    }

    public void OnAccelerateBrakeRelease()
    {
        vehicleController.OnAccelerateBrakeRelease();
    }

    public void OnSteerLeftButtonPressed()
    {
        vehicleController.OnSteerLeftButtonPressed();
    }

    public void OnSteerRightButtonPressed()
    {
        vehicleController.OnSteerRightButtonPressed();
    }

    public void OnSteerRelease()
    {
        vehicleController.OnSteerRelease();
    }

    public void ExitCar()
    {
        vehicleController.ExitCar();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        vehicleController.HandleTriggerEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        vehicleController.HandleTriggerExit(other);
    }

    public bool HasCollectedCollectible(string collectibleId)
    {
        return persistence.HasCollectedCollectible(collectibleId);
    }

    public void RegisterCollectible(string collectibleId)
    {
        persistence.RegisterCollectible(collectibleId);
    }

    public IReadOnlyList<string> CollectedCollectibleIds => persistence.CollectedCollectibleIds;

    public void SetAutoSaveInterval(float seconds)
    {
        persistence.AutoSaveIntervalSeconds = seconds;
    }

    private static float ReadNormalizedPreference(string key, float defaultValue)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(key, defaultValue));
    }
}
