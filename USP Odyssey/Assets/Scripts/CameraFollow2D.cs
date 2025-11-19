using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float defaultLerpSpeed = 5f;
    [SerializeField] private float busSmoothTime = 0.2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private bool useBusMode;
    private Vector3 smoothVelocity;

    private void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        if (useBusMode)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, busSmoothTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, defaultLerpSpeed * Time.deltaTime);
        }
    }

    public void SetTarget(Transform newTarget, bool busMode)
    {
        target = newTarget;
        useBusMode = busMode;
        smoothVelocity = Vector3.zero;
    }

    public Transform CurrentTarget => target;
}
