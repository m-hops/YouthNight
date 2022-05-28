using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(MouseLook))]
public class InputManager : MonoBehaviour
{

    Movement movement;
    MouseLook mouseLook;

    PlayerControls controls;
    PlayerControls.GroundMovementActions groundMovement;

    Vector2 horizontalInput;
    Vector2 mouseInput;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        mouseLook = GetComponent<MouseLook>();

        controls = new PlayerControls();
        groundMovement = controls.GroundMovement;

        groundMovement.HorizontalMovement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
        groundMovement.Jump.performed += _ => movement.OnJumpPressed();

        groundMovement.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        groundMovement.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

    }

    private void Update()
    {
        movement.ReceiveInput(horizontalInput);
        mouseLook.ReceiveInput(mouseInput);
    }

    private void OnEnable ()
    {
        controls.Enable();
    }

    private void OnDestroy ()
    {
        controls.Disable();
    }
}
