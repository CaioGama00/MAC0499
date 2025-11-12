using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{   
    public  float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20;

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

        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;

        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        if (carRigidbody2d.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
            return;

        if (accelerationInput == 0)
            carRigidbody2d.linearDamping = Mathf.Lerp(carRigidbody2d.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        else carRigidbody2d.linearDamping = 0;

        Vector2 engineForceVector = transform.up * accelerationInput *accelerationFactor;
        carRigidbody2d.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {   
        float minSpeedBeforeAllowTurningFactor = carRigidbody2d.linearVelocity.magnitude / 8;
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;
        carRigidbody2d.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2d.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2d.linearVelocity, transform.right);

        carRigidbody2d.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }
    public void SetInputVector(Vector2 inputVector)
    {   
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }
}
