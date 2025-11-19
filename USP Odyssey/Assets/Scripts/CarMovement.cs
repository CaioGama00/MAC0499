using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Handling")]
    [SerializeField] private float driftFactor = 0.92f;
    [SerializeField] private float traction = 0.85f;
    [SerializeField] private float turnFactor = 4.25f;
    [SerializeField] private float minTurnSpeed = 6f;
    [SerializeField] private float turnResponsiveness = 0.35f;
    [SerializeField] private float stationaryTurnThreshold = 0.5f;

    [Header("Power")]
    [SerializeField] private float forwardAcceleration = 100f;
    [SerializeField] private float reverseAcceleration = 80f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float brakingDrag = 3f;
    [SerializeField] private float coastingDrag = 1.2f;

    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;
    float velocityVsUp = 0;
    Rigidbody2D carRigidbody2d;
    // Start is called before the first frame update

    void Awake()
    {
        carRigidbody2d = GetComponent<Rigidbody2D>();
    }
    
    void FixedUpdate()
    {
        ApplyEngineForce();
        KillOrthogonalVelocity();
        ApplySteering();
    }

    void ApplyEngineForce()
    {
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2d.linearVelocity);

        if (velocityVsUp > maxSpeed && accelerationInput > 0f)
        {
            accelerationInput = 0f;
        }
        else if (velocityVsUp < -maxSpeed * 0.4f && accelerationInput < 0f)
        {
            accelerationInput = 0f;
        }

        float targetDrag = Mathf.Abs(accelerationInput) > 0.01f ? 0f : coastingDrag;
        if (Mathf.Sign(accelerationInput) != Mathf.Sign(velocityVsUp) && Mathf.Abs(velocityVsUp) > 1f)
        {
            targetDrag = brakingDrag;
        }

        carRigidbody2d.linearDamping = Mathf.Lerp(carRigidbody2d.linearDamping, targetDrag, Time.fixedDeltaTime * 5f);

        float currentAcceleration = accelerationInput >= 0f ? forwardAcceleration : reverseAcceleration;
        Vector2 engineForceVector = transform.up * accelerationInput * currentAcceleration;
        carRigidbody2d.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        float speed = carRigidbody2d.linearVelocity.magnitude;
        if (speed < stationaryTurnThreshold && Mathf.Abs(accelerationInput) < 0.05f)
        {
            return;
        }

        float speedFactor = Mathf.InverseLerp(stationaryTurnThreshold, minTurnSpeed, speed);
        float baseResponsiveness = Mathf.Lerp(turnResponsiveness, 1f, speedFactor);

        rotationAngle -= steeringInput * turnFactor * baseResponsiveness;
        carRigidbody2d.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2d.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2d.linearVelocity, transform.right);

        carRigidbody2d.linearVelocity = forwardVelocity + rightVelocity * driftFactor;

        Vector2 desiredVelocity = forwardVelocity + rightVelocity * traction;
        carRigidbody2d.linearVelocity = Vector2.Lerp(carRigidbody2d.linearVelocity, desiredVelocity, Time.fixedDeltaTime * 5f);
    }
    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = Mathf.Clamp(inputVector.y, -1f, 1f);
    }

}
