using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

[RequireComponent(typeof(CarMovement))]
public class CarInputHandler : MonoBehaviour
{   
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [Tooltip("Normalize diagonal input so that moving diagonally isn't faster than straight lines.")]
    [SerializeField] private bool normalizeDiagonalInput = true;

    private CarMovement carMovement;

    private void Awake()
    {
        if (!TryGetComponent(out carMovement))
        {
            Debug.LogError("CarMovement component missing. Disabling CarInputHandler.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (carMovement == null)
        {
            return;
        }

        Vector2 inputVector = new Vector2(
            Input.GetAxisRaw(horizontalAxis),
            Input.GetAxisRaw(verticalAxis));

        if (normalizeDiagonalInput && inputVector.sqrMagnitude > 1f)
        {
            inputVector = inputVector.normalized;
        }

        carMovement.SetInputVector(inputVector);
    }
}
