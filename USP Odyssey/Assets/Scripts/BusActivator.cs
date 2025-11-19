using UnityEngine;


/// Enables/disables an existing BusMovement instance based on the player's proximity.
/// Keeps buses dormant when the player is far away to save CPU/memory.

public class BusActivator : MonoBehaviour
{
    [SerializeField] private BusMovement bus;
    [SerializeField] private Transform explicitTarget;
    [SerializeField] private float activationDistance = 120f;
    [SerializeField] private float deactivationDistance = 150f;

    private static Transform globalTarget;

    private void Awake()
    {
        if (bus == null)
        {
            bus = GetComponentInChildren<BusMovement>();
        }
    }

    private void Update()
    {
        if (bus == null)
        {
            return;
        }

        Transform target = explicitTarget != null ? explicitTarget : GetGlobalTarget();
        if (target == null)
        {
            return;
        }

        float distance = Vector3.Distance(target.position, bus.transform.position);
        if (!bus.gameObject.activeSelf && distance <= activationDistance)
        {
            ToggleBus(true);
        }
        else if (bus.gameObject.activeSelf && distance >= deactivationDistance)
        {
            ToggleBus(false);
        }
    }

    public static void SetGlobalTrackingTarget(Transform newTarget)
    {
        globalTarget = newTarget;
    }

    private Transform GetGlobalTarget()
    {
        if (globalTarget != null)
        {
            return globalTarget;
        }

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            globalTarget = player.transform;
        }

        return globalTarget;
    }

    private void ToggleBus(bool state)
    {
        bus.gameObject.SetActive(state);
    }

    private void OnDrawGizmosSelected()
    {
        if (bus == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(bus.transform.position, activationDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bus.transform.position, deactivationDistance);
    }
}
