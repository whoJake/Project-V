using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private StatHandler statHandler;

    public float groundAccelerationTime;
    private Vector3 velocity;
    public float gravity;
    public float terminalVelocity = 10f;
    public float jumpHeight;
    private bool isFalling;
    private bool isJumping;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    private void Awake() {
        statHandler = gameObject.GetComponent<StatHandler>();
    }

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    void HandleMovement() {
        Vector2 inputVectorRaw = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")).normalized;
        dampedInput = Vector2.SmoothDamp(dampedInput, inputVectorRaw, ref dampedInputVelocity, groundAccelerationTime);

        Vector3 transformDirection = (transform.forward * dampedInput.x) + (transform.right * dampedInput.y);
        velocity.x = transformDirection.x * statHandler.movementSpeed;
        velocity.z = transformDirection.z * statHandler.movementSpeed;
    }

    void HandleGravity() {
        if (controller.isGrounded) {
            velocity.y = -0.1f;
        } else {
            velocity.y -= gravity * 9.81f * Time.deltaTime;
            if (velocity.y < -terminalVelocity) velocity.y = -terminalVelocity;
        }
    }

    void HandleJump() {
        if (controller.isGrounded) {
            isJumping = false;
            isFalling = false;
        }else if(velocity.y < 0) {
            isFalling = true;
        }

        bool jumpKeyPressed = Input.GetAxisRaw("Jump") > 0;
        if(controller.isGrounded && jumpKeyPressed) {
            isJumping = true;
            velocity.y = jumpHeight; //Change to calculate this based on jumpheight and jumptime
        }
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleJump();

        controller.Move(velocity * Time.deltaTime);
    }
}
