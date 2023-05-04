using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private StatHandler statHandler;

    [SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private float gravity;
    [SerializeField] [Tooltip("Maximum downward velocity caused by gravity")]
    private float terminalVelocity;
    [SerializeField]
    private float jumpHeight;
    [SerializeField] [Tooltip("Time taken to accelerate to player movement speed")]
    private float groundAccelerationTime;
    [SerializeField]
    private float hitGroundEventThreshold;


    private bool isFalling;
    private bool isJumping;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    //Events
    public delegate void onHitGround(Vector3 position, float downwardSpeed);
    public event onHitGround OnHitGround;

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
        //This accounts for hitting head
        if(isJumping && controller.velocity.y == 0) {
            velocity.y = 0;
        }

        //Hit ground event
        if(isFalling && controller.isGrounded && 
           velocity.y <= -hitGroundEventThreshold) {

           if(OnHitGround != null) OnHitGround.Invoke(transform.position, velocity.y);
        }

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
