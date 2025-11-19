using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BusLine
{
    public string lineName;
    public Color lineColor;
    public List<Transform> waypoints;
}

public class BusMovement : MonoBehaviour
{
    public BusLine busLine;
    [Min(0f)] public float speed = 5f;
    [Min(0f)] public float rotationSpeed = 3f;
    [Min(0f)] public float stopDuration = 0f;
    [Min(0f)] public float detectionRange = 7f;   // Distance to detect obstacles
    public LayerMask obstacleLayer;     // Define which objects are obstacles
    public int currentWaypointIndex = 0;
    [Header("Route Settings")]
    [Tooltip("Optional explicit final stop. If set, the bus will consider this waypoint as the final stop and trigger auto-exit there.")]
    public Transform finalWaypoint;
    [SerializeField, Min(0.01f)] private float waypointArrivalThreshold = 0.1f;
    [SerializeField, Min(0f)] private float obstacleStuckTimeout = 5f;

    // Fired when the bus reaches its final stop in the route.
    public event Action<BusMovement> FinalStopReached;

    private bool isWaiting = false;
    private float obstacleFirstDetectedTime = -1f;
    private bool isStuck = false;
    private bool obstacleDetected = false; // Tracks if an obstacle is detected
    private Vector3 direction;
    private int finalStopIndex = -1; // Index of the final stop waypoint
    private bool routeReady;
    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        routeReady = ValidateRoute();
        if (!routeReady || !TryGetCurrentWaypoint(out Transform startingWaypoint))
        {
            enabled = false;
            return;
        }

        transform.position = startingWaypoint.position;
        ResolveFinalStopIndex();
        PlayEngineSoundIfAvailable();
    }

    void Update()
    {
        if (!routeReady)
        {
            return;
        }

        if (!isWaiting && !obstacleDetected)
        {
            MoveAndRotateBus();
        }

        CheckForObstacles();
    }

    void CheckForObstacles()
    {
        if (direction.sqrMagnitude <= float.Epsilon)
        {
            return;
        }

        float boxWidth = 4f; // Width of the detection area
        float forwardOffset = 6.0f; // Distance to move the starting position forward
        Vector2 boxSize = new Vector2(boxWidth, 0.5f);

        // Calculate the forward offset based on the bus's rotation
        Vector2 forwardDirection = direction.normalized;
        Vector2 raycastOrigin = (Vector2)transform.position + forwardDirection * forwardOffset;

        // Perform the BoxCast from the adjusted position
        RaycastHit2D hit = Physics2D.BoxCast(raycastOrigin, boxSize, 0, forwardDirection, detectionRange, obstacleLayer);

        // Debugging: Draw the ray in the Scene view
        Debug.DrawRay(raycastOrigin, forwardDirection * detectionRange, Color.red);

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            if (!obstacleDetected)
            {
                obstacleFirstDetectedTime = Time.time;
            }
            obstacleDetected = true;

            if (obstacleFirstDetectedTime >= 0 && Time.time - obstacleFirstDetectedTime > obstacleStuckTimeout)
            {
                isStuck = true;
            }
        }
        else
        {
            obstacleDetected = false;
            isStuck = false;
            obstacleFirstDetectedTime = -1f;
        }
        if (isStuck)
        {
            // Try to move around the obstacle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += 45f; // Try to move 45 degrees around the obstacle
            direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        }
    }

    void MoveAndRotateBus()
    {
        if (!TryGetCurrentWaypoint(out Transform targetWaypoint))
        {
            return;
        }
        direction = (targetWaypoint.position - transform.position).normalized;

        float adjustedSpeed = speed;
        transform.position += direction * adjustedSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 180f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if ((transform.position - targetWaypoint.position).sqrMagnitude < waypointArrivalThreshold * waypointArrivalThreshold)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private System.Collections.IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        // If we are at the final stop, notify listeners (e.g., Player) so they can auto-exit.
        if (currentWaypointIndex == finalStopIndex)
        {
            FinalStopReached?.Invoke(this);
        }

        // Only wait if this waypoint is a designated Stop
        if (busLine.waypoints[currentWaypointIndex].CompareTag("Stop"))
        {
            yield return new WaitForSeconds(stopDuration);
        }

        isWaiting = false;
        currentWaypointIndex = (currentWaypointIndex + 1) % busLine.waypoints.Count;
    }

    // Returns true if the bus is either waiting at a stop or stopped due to an obstacle
    public bool isStopped()
    {
        return isWaiting || obstacleDetected;
    }

    // Returns the current movement direction of the bus
    public Vector3 currentDirection()
    {
        return direction;
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || Camera.current == null)
        {
            return;
        }
        if (busLine != null && busLine.waypoints != null && busLine.waypoints.Count > 1)
        {
            Gizmos.color = busLine.lineColor;
            for (int i = 0; i < busLine.waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(busLine.waypoints[i].position, busLine.waypoints[i + 1].position);
            }
        }

        // Draw the detection ray 
        if (direction != Vector3.zero)
        {
            Gizmos.color = Color.red;

            float forwardOffset = 6.0f; // Move the raycast start position slightly forward
            Vector3 forwardDirection = direction.normalized;
            Vector3 raycastOrigin = transform.position + forwardDirection * forwardOffset;

            // Draw the raycast line in front of the bus
            Gizmos.DrawLine(raycastOrigin, raycastOrigin + forwardDirection * detectionRange);
        }

        // Draw circles around stop waypoints with the same color as the bus line
        if (busLine != null && busLine.waypoints != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                foreach (Transform waypoint in busLine.waypoints)
                {
                    if (waypoint.CompareTag("Stop"))
                    {
                        Gizmos.color = busLine.lineColor; // Match bus line color
                        DrawGizmoCircle(waypoint.position);
                    }
                }
            }
        }
    }

    // Function to draw a smooth circle
    void DrawGizmoCircle(Vector3 center)
    {   
        float circleSize = 7.0f;
        int segments = 50; // More segments for a smoother circle
        float angleStep = 2 * Mathf.PI / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0) * circleSize;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleSize;

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    private bool ValidateRoute()
    {
        if (!RouteReady)
        {
            Debug.LogWarning($"BusMovement on {name} requires a bus line with at least one waypoint.", this);
            return false;
        }

        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, busLine.waypoints.Count - 1);
        return true;
    }

    private bool TryGetCurrentWaypoint(out Transform waypoint)
    {
        waypoint = null;
        if (!RouteReady)
        {
            return false;
        }

        int waypointCount = busLine.waypoints.Count;
        for (int offset = 0; offset < waypointCount; offset++)
        {
            int candidateIndex = (currentWaypointIndex + offset) % waypointCount;
            Transform candidate = busLine.waypoints[candidateIndex];
            if (candidate == null)
            {
                continue;
            }

            if (candidateIndex != currentWaypointIndex)
            {
                currentWaypointIndex = candidateIndex;
            }

            waypoint = candidate;
            return true;
        }

        Debug.LogWarning($"All waypoints assigned to {name} are null. Disabling BusMovement.", this);
        enabled = false;
        return false;
    }

    private void ResolveFinalStopIndex()
    {
        if (!RouteReady)
        {
            finalStopIndex = -1;
            return;
        }

        finalStopIndex = -1;
        if (finalWaypoint != null)
        {
            int idx = busLine.waypoints.IndexOf(finalWaypoint);
            if (idx >= 0)
            {
                finalStopIndex = idx;
            }
            else
            {
                Debug.LogWarning($"Final waypoint '{finalWaypoint.name}' is not in the waypoints list for {name}. Falling back to last Stop or last waypoint.", this);
            }
        }

        if (finalStopIndex == -1)
        {
            for (int i = 0; i < busLine.waypoints.Count; i++)
            {
                Transform waypoint = busLine.waypoints[i];
                if (waypoint != null && waypoint.CompareTag("Stop"))
                {
                    finalStopIndex = i;
                }
            }
        }

        if (finalStopIndex == -1)
        {
            finalStopIndex = busLine.waypoints.Count - 1;
        }
    }

    private void PlayEngineSoundIfAvailable()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length > 1)
        {
            audioSources[1].Play();
        }
    }

    private bool RouteReady => busLine != null && busLine.waypoints != null && busLine.waypoints.Count > 0;
}
