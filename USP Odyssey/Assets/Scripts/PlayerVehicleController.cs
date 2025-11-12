using System;
using System.Collections;
using UnityEngine;

public class PlayerVehicleController : MonoBehaviour
{
    private const float DefaultCarExitOffsetX = 1f;

    [SerializeField] private float exitCooldownDuration = 1f;

    private PlayerMovement movement;
    private GameObject movementJoystickPanel;
    private GameObject inBusPanel;
    private GameObject exitBusPanel;
    [SerializeField] private GameObject[] inBusPanelElementsToDisable;
    private bool[] inBusPanelElementsDefaultStates = Array.Empty<bool>();
    private bool exitBusPanelDefaultActiveState;
    private GameObject enterCarPanel;
    private GameObject enterBusPanel;
    private GameObject miniMap;
    private GameObject[] invisibleWalls;
    private AudioSource buttonClickSound;
    private AudioSource panelSound;
    private CameraFollow2D mainCamera;
    private Camera playerCamera;

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] playerColliders;

    private bool recentlyExited;
    private bool exitButtonPressed;
    private bool isInCar;
    private bool isInBus;

    private CarMovement carMovement;
    private CarMovement cachedCarMovement;
    private Transform cachedCarTransform;
    private Rigidbody2D cachedCarBody;
    private AudioSource[] cachedCarAudioSources = Array.Empty<AudioSource>();
    private BusMovement busMovement;
    private Vector2 inputVector = Vector2.zero;
    private Vector2 keyboardInputVector = Vector2.zero;
    private Vector2 uiInputVector = Vector2.zero;
    private Coroutine exitCooldownRoutine;

    public bool IsInBus => isInBus;

    public void Configure(
        PlayerMovement playerMovement,
        GameObject joystickPanel,
        GameObject busPanel,
        GameObject busExitPanel,
        GameObject carPanel,
        GameObject busEnterPanel,
        GameObject miniMapObject,
        GameObject[] walls,
        AudioSource buttonSound,
        AudioSource busPanelSound,
        CameraFollow2D followCamera,
        Camera targetCamera,
        float cooldownDuration,
        GameObject[] panelElementsToDisableWhileDriving)
    {
        movement = playerMovement;
        movementJoystickPanel = joystickPanel;
        inBusPanel = busPanel;
        exitBusPanel = busExitPanel;
        enterCarPanel = carPanel;
        enterBusPanel = busEnterPanel;
        miniMap = miniMapObject;
        invisibleWalls = walls;
        buttonClickSound = buttonSound;
        panelSound = busPanelSound;
        mainCamera = followCamera;
        playerCamera = targetCamera;
        exitCooldownDuration = cooldownDuration;
        inBusPanelElementsToDisable = panelElementsToDisableWhileDriving;

        if (inBusPanelElementsToDisable != null && inBusPanelElementsToDisable.Length > 0)
        {
            inBusPanelElementsDefaultStates = new bool[inBusPanelElementsToDisable.Length];
            for (int i = 0; i < inBusPanelElementsToDisable.Length; i++)
            {
                GameObject element = inBusPanelElementsToDisable[i];
                inBusPanelElementsDefaultStates[i] = element != null && element.activeSelf;
            }
        }
        else
        {
            inBusPanelElementsDefaultStates = Array.Empty<bool>();
        }

        exitBusPanelDefaultActiveState = exitBusPanel != null && exitBusPanel.activeSelf;
    }

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        playerColliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void OnDisable()
    {
        if (busMovement != null)
        {
            busMovement.FinalStopReached -= OnBusFinalStopReached;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteractRequest();
        }

        if (isInCar)
        {
            HandleCarKeyboardInput();
        }
        else if (exitButtonPressed)
        {
            ExitBus();
        }
    }

    public void ButtonPressed()
    {
        exitButtonPressed = true;
    }

    public void HandleTriggerEnter(Collider2D other)
    {
        if (other.CompareTag("Car") && !recentlyExited)
        {
            PlaySound(panelSound);
            ShowEnterCarPanel(other);
        }
        else if (other.CompareTag("Bus") && !recentlyExited)
        {
            PlaySound(panelSound);
            ShowEnterBusPanel(other);
        }
    }

    public void HandleTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            HideEnterCarPanel();
            if (!isInCar)
            {
                carMovement = null;
            }
        }
        else if (other.CompareTag("Bus"))
        {
            HideEnterBusPanel();
            if (!isInBus)
            {
                busMovement = null;
            }
        }
    }

    public void OnYesButtonClicked()
    {
        if (carMovement == null)
        {
            Debug.LogWarning("Car movement script is missing. Cannot enter car.");
            return;
        }

        PlaySound(buttonClickSound);
        AudioSource[] audioSources = carMovement.GetComponents<AudioSource>();
        if (audioSources.Length > 0)
        {
            PlaySound(audioSources[0]);
            StartCoroutine(WaitForCarEngineStartToEnd(audioSources));
        }
        EnterCar();
        HideEnterCarPanel();
    }

    public void OnNoButtonClicked()
    {
        PlaySound(buttonClickSound);
        HideEnterCarPanel();
    }

    public void OnYesBusButtonClicked()
    {
        if (busMovement == null)
        {
            Debug.LogWarning("Bus movement script is missing. Cannot enter bus.");
            return;
        }

        PlaySound(buttonClickSound);
        EnterBus();
        HideEnterBusPanel();
    }

    public void OnNoBusButtonClicked()
    {
        PlaySound(buttonClickSound);
        HideEnterBusPanel();
    }

    public void OnAcceleratePress()
    {
        uiInputVector.y = -1.0f;
        ApplyCarInput();
    }

    public void OnBrakePress()
    {
        uiInputVector.y = 1.0f;
        ApplyCarInput();
    }

    public void OnAccelerateBrakeRelease()
    {
        uiInputVector.y = 0.0f;
        ApplyCarInput();
    }

    public void OnSteerLeftButtonPressed()
    {
        uiInputVector.x = -1.0f;
        ApplyCarInput();
    }

    public void OnSteerRightButtonPressed()
    {
        uiInputVector.x = 1.0f;
        ApplyCarInput();
    }

    public void OnSteerRelease()
    {
        uiInputVector.x = 0.0f;
        ApplyCarInput();
    }

    public void HandleEnteredBus(BusMovement bus)
    {
        busMovement = bus;
    }

    public void HandleExitedBus()
    {
        busMovement = null;
    }

    public void HandleEnteredCar(CarMovement car)
    {
        carMovement = car;
    }

    public void HandleExitedCar()
    {
        carMovement = null;
    }

    private void HandleInteractRequest()
    {
        if (isInCar)
        {
            ExitCar();
        }
        else if (isInBus)
        {
            ExitBus();
        }
        else if (!recentlyExited)
        {
            if (carMovement != null && enterCarPanel != null && enterCarPanel.activeSelf)
            {
                OnYesButtonClicked();
            }
            else if (busMovement != null && enterBusPanel != null && enterBusPanel.activeSelf)
            {
                OnYesBusButtonClicked();
            }
        }
    }

    private void HandleCarKeyboardInput()
    {
        float steer = 0f;
        if (Input.GetKey(KeyCode.A)) { steer -= 1f; }
        if (Input.GetKey(KeyCode.D)) { steer += 1f; }

        float throttle = 0f;
        if (Input.GetKey(KeyCode.W)) { throttle -= 1f; }
        if (Input.GetKey(KeyCode.S)) { throttle += 1f; }

        keyboardInputVector.x = Mathf.Clamp(steer, -1f, 1f);
        keyboardInputVector.y = Mathf.Clamp(throttle, -1f, 1f);

        ApplyCarInput(forceUpdate: true);
    }

    private void EnterBus()
    {
        if (busMovement == null)
        {
            return;
        }

        ResetInputState();
        exitButtonPressed = false;
        isInBus = true;
        transform.position = busMovement.transform.position;
        if (mainCamera != null)
        {
            mainCamera.player = busMovement.transform;
        }
        if (playerCamera != null)
        {
            playerCamera.orthographicSize = 20f;
        }
        SetPlayerVisualsActive(false);
        movement?.SetMovementLocked(true);
        movementJoystickPanel?.SetActive(false);
        inBusPanel?.SetActive(true);
        SetBusPanelElementsActive(false);
        ApplyExitBusPanelDefaultState();
        if (busMovement != null)
        {
            busMovement.FinalStopReached += OnBusFinalStopReached;
        }
        Debug.Log("Entered the bus");
    }

    private void ExitBus()
    {
        if (busMovement == null || !busMovement.isStopped())
        {
            return;
        }

        ResetInputState();
        Vector3 exitOffset = busMovement.currentDirection() * busMovement.speed;
        transform.position = busMovement.transform.position + exitOffset;
        SetPlayerVisualsActive(true);
        if (mainCamera != null)
        {
            mainCamera.player = transform;
        }
        if (playerCamera != null)
        {
            playerCamera.orthographicSize = 12f;
        }
        exitButtonPressed = false;
        isInBus = false;
        ApplyExitBusPanelDefaultState();
        movementJoystickPanel?.SetActive(true);
        inBusPanel?.SetActive(false);
        RestoreBusPanelElementsDefaultState();
        movement?.SetMovementLocked(false);
        Debug.Log("Exited the bus");
        recentlyExited = true;
        StartExitCooldown();
        if (busMovement != null)
        {
            busMovement.FinalStopReached -= OnBusFinalStopReached;
        }
    }

    private void EnterCar()
    {
        if (carMovement == null)
        {
            Debug.LogWarning("Attempted to enter a car but no CarMovement was assigned.");
            return;
        }

        cachedCarMovement = carMovement;
        CacheCarReferences();

        ResetInputState();
        carMovement.SetInputVector(Vector2.zero);
        carMovement.enabled = true;
        isInCar = true;
        transform.position = carMovement.transform.position;
        if (mainCamera != null)
        {
            mainCamera.player = carMovement.transform;
        }
        if (playerCamera != null)
        {
            playerCamera.orthographicSize = 20f;
        }
        miniMap?.SetActive(false);
        SetPlayerVisualsActive(false);
        movement?.SetMovementLocked(true);
        Debug.Log("Entered the car");
        UpdateInvisibleWalls(true);
    }

    public void ExitCar()
    {
        CarMovement movementToControl = carMovement != null ? carMovement : cachedCarMovement;
        Transform carTransform = carMovement != null ? carMovement.transform : cachedCarTransform;
        Rigidbody2D carBody = carMovement != null
            ? carMovement.GetComponent<Rigidbody2D>()
            : cachedCarBody;

        isInCar = false;
        ResetInputState();

        if (movementToControl != null)
        {
            movementToControl.SetInputVector(Vector2.zero);
        }

        Vector3 exitOffsetVector = new Vector3(DefaultCarExitOffsetX, 0f, 0f);
        Vector3 exitPosition = carTransform != null
            ? carTransform.position + exitOffsetVector
            : transform.position + exitOffsetVector;
        transform.position = exitPosition;

        SetPlayerVisualsActive(true);
        movementJoystickPanel?.SetActive(true);

        if (mainCamera != null)
        {
            mainCamera.player = transform;
        }
        miniMap?.SetActive(true);
        if (playerCamera != null)
        {
            playerCamera.orthographicSize = 12f;
        }

        if (carBody != null)
        {
            carBody.linearVelocity = Vector2.zero;
            carBody.angularVelocity = 0f;
        }

        if (movementToControl != null)
        {
            movementToControl.enabled = false;
            StopCarAudioSources(movementToControl.GetComponents<AudioSource>());
        }
        else
        {
            StopCarAudioSources(cachedCarAudioSources);
        }

        movement?.SetMovementLocked(false);
        Debug.Log("Exited the car");
        recentlyExited = true;
        exitButtonPressed = false;
        StartExitCooldown();
        UpdateInvisibleWalls(false);
        carMovement = null;
        cachedCarMovement = null;
        cachedCarTransform = null;
        cachedCarBody = null;
        cachedCarAudioSources = Array.Empty<AudioSource>();
    }

    private void StartExitCooldown()
    {
        if (exitCooldownRoutine != null)
        {
            StopCoroutine(exitCooldownRoutine);
        }
        exitCooldownRoutine = StartCoroutine(ExitCooldown());
    }

    private IEnumerator ExitCooldown()
    {
        yield return new WaitForSeconds(exitCooldownDuration);
        recentlyExited = false;
    }

    private IEnumerator WaitForCarEngineStartToEnd(AudioSource[] audioSources)
    {
        if (audioSources.Length == 0)
        {
            yield break;
        }

        AudioSource engineStartSound = audioSources[0];
        if (engineStartSound.clip != null)
        {
            yield return new WaitForSeconds(engineStartSound.clip.length);
        }
        if (isInCar && audioSources.Length > 1)
        {
            PlaySound(audioSources[1]);
        }
    }

    private void ShowEnterCarPanel(Collider2D other)
    {
        carMovement = other.GetComponent<CarMovement>();
        if (enterCarPanel != null)
        {
            enterCarPanel.SetActive(true);
        }
    }

    private void ShowEnterBusPanel(Collider2D other)
    {
        busMovement = other.GetComponent<BusMovement>();
        if (enterBusPanel != null)
        {
            enterBusPanel.SetActive(true);
        }
    }

    private void CacheCarReferences()
    {
        if (carMovement == null)
        {
            cachedCarTransform = null;
            cachedCarBody = null;
            cachedCarAudioSources = Array.Empty<AudioSource>();
            return;
        }

        cachedCarTransform = carMovement.transform;
        cachedCarBody = carMovement.GetComponent<Rigidbody2D>();
        cachedCarAudioSources = carMovement.GetComponents<AudioSource>() ?? Array.Empty<AudioSource>();
    }

    private void SetPlayerVisualsActive(bool isActive)
    {
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].enabled = isActive;
                }
            }
        }

        if (playerColliders != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                if (playerColliders[i] != null)
                {
                    playerColliders[i].enabled = isActive;
                }
            }
        }
    }

    private void StopCarAudioSources(AudioSource[] audioSources)
    {
        if (audioSources == null)
        {
            return;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].Stop();
            }
        }
    }

    private void ApplyExitBusPanelDefaultState()
    {
        if (exitBusPanel != null)
        {
            exitBusPanel.SetActive(exitBusPanelDefaultActiveState);
        }
    }

    private void SetBusPanelElementsActive(bool isActive)
    {
        if (inBusPanelElementsToDisable == null)
        {
            return;
        }

        for (int i = 0; i < inBusPanelElementsToDisable.Length; i++)
        {
            GameObject element = inBusPanelElementsToDisable[i];
            if (element != null)
            {
                element.SetActive(isActive);
            }
        }
    }

    private void RestoreBusPanelElementsDefaultState()
    {
        if (inBusPanelElementsToDisable == null || inBusPanelElementsDefaultStates == null)
        {
            return;
        }

        int length = Mathf.Min(inBusPanelElementsToDisable.Length, inBusPanelElementsDefaultStates.Length);
        for (int i = 0; i < length; i++)
        {
            GameObject element = inBusPanelElementsToDisable[i];
            if (element != null)
            {
                element.SetActive(inBusPanelElementsDefaultStates[i]);
            }
        }
    }

    private void ApplyCarInput(bool forceUpdate = false)
    {
        if (carMovement == null || !isInCar)
        {
            return;
        }

        Vector2 combined = uiInputVector;

        if (keyboardInputVector.x != 0f)
        {
            combined.x = keyboardInputVector.x;
        }

        if (keyboardInputVector.y != 0f)
        {
            combined.y = keyboardInputVector.y;
        }

        bool changed = combined != inputVector;
        if (changed || forceUpdate)
        {
            inputVector = combined;
            carMovement.SetInputVector(inputVector);
        }
        else if (inputVector == Vector2.zero)
        {
            carMovement.SetInputVector(Vector2.zero);
        }
    }

    private void ResetInputState()
    {
        keyboardInputVector = Vector2.zero;
        uiInputVector = Vector2.zero;
        inputVector = Vector2.zero;
    }

    private void HideEnterCarPanel()
    {
        if (enterCarPanel != null)
        {
            enterCarPanel.SetActive(false);
        }
    }

    private void HideEnterBusPanel()
    {
        if (enterBusPanel != null)
        {
            enterBusPanel.SetActive(false);
        }
    }

    private void PlaySound(AudioSource sound)
    {
        if (sound != null)
        {
            sound.Play();
        }
    }

    private void UpdateInvisibleWalls(bool enable)
    {
        if (invisibleWalls == null)
        {
            return;
        }

        foreach (GameObject wall in invisibleWalls)
        {
            if (wall != null)
            {
                wall.SetActive(enable);
            }
        }
    }

    private void OnBusFinalStopReached(BusMovement bus)
    {
        if (isInBus && busMovement == bus)
        {
            ExitBus();
        }
    }
}
