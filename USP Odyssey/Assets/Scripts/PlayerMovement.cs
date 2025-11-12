using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float stuckThreshold = 3f;

    private MovementJoystick movementJoystick;
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource walkAudioSource;
    private Vector3 spawnPoint;
    private bool isMovementLocked;
    private bool isWalking;
    private bool isStuck;
    private float stuckTime;

    public void Configure(MovementJoystick joystick, float speed)
    {
        movementJoystick = joystick;
        playerSpeed = speed;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        walkAudioSource = GetComponent<AudioSource>();
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (isMovementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateWalkingSound(Vector2.zero);
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
            return;
        }

        Vector2 dir = GetInput();
        rb.linearVelocity = new Vector2(dir.x * 1.15f * playerSpeed, dir.y * playerSpeed);
        isWalking = dir.sqrMagnitude > 0f;

        if (animator != null)
        {
            animator.SetFloat("Horizontal", dir.x);
            animator.SetFloat("Vertical", dir.y);
            animator.SetBool("isMoving", isWalking);
        }

        UpdateWalkingSound(dir);
        CheckStuck();
    }

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;
        if (locked)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateWalkingSound(Vector2.zero);
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void ReturnToSpawnPoint()
    {
        transform.position = spawnPoint;
        stuckTime = 0f;
        isStuck = false;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void WarpTo(Vector3 position)
    {
        transform.position = position;
    }

    private Vector2 GetInput()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A)) { dir.x = -1; }
        else if (Input.GetKey(KeyCode.D)) { dir.x = 1; }
        if (Input.GetKey(KeyCode.W)) { dir.y = 1; }
        else if (Input.GetKey(KeyCode.S)) { dir.y = -1; }
        if (movementJoystick != null && movementJoystick.joystickVec != Vector2.zero)
        {
            dir.x += movementJoystick.joystickVec.x;
            dir.y += movementJoystick.joystickVec.y;
        }
        if (dir.magnitude > 1f)
        {
            dir.Normalize();
        }
        return dir;
    }

    private void UpdateWalkingSound(Vector2 dir)
    {
        if (walkAudioSource == null)
        {
            return;
        }

        if (dir.sqrMagnitude > 0f)
        {
            if (!walkAudioSource.isPlaying)
            {
                walkAudioSource.Play();
            }
        }
        else
        {
            walkAudioSource.Stop();
        }
    }

    private void CheckStuck()
    {
        if (isWalking && isStuck)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime > stuckThreshold)
            {
                Debug.Log("Player is stuck, returning to spawn point.");
                ReturnToSpawnPoint();
            }
        }
        else
        {
            stuckTime = 0f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isStuck = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isStuck = false;
    }
}
