using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] float speed = 11f;
    Vector2 horizontalInput;

    [SerializeField] float jumpHeight = 3.5f;
    bool jump;

    [SerializeField] float gravity = -30f;
    Vector3 VerticalVelocity = Vector3.zero;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //Jump Mechanic
        if (jump)
        {
            if (controller.isGrounded)
            {
                VerticalVelocity.y = Mathf.Sqrt(-2f * jumpHeight * gravity);
            }
            jump = false;
        }
        else if (controller.isGrounded)
        {
            // -1 to move down toward the ground so controller.isGrounded stays true 
            // even with small deltatimes
            VerticalVelocity.y = -1;
        }

        //Handles movement on X & Y
        Vector3 horizontalVelocity = (transform.right * horizontalInput.x + transform.forward * horizontalInput.y) * speed;
        controller.Move(horizontalVelocity * Time.deltaTime);

        //Applies gravity to character
        VerticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(VerticalVelocity * Time.deltaTime);
    }

    public void ReceiveInput (Vector2 _horizontalInput)
    {
        horizontalInput = _horizontalInput;
    }

    public void OnJumpPressed()
    {
        jump = true;
    }
}
