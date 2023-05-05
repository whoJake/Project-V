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

    [SerializeField] [Tooltip("Time it takes to reach the apex of jump")]
    private float timeToApex;

    [SerializeField]
    private float gravityWhilstFallingMultiplier;

    [SerializeField] [Tooltip("Maximum downward velocity caused by gravity")]
    private float terminalVelocity;

    [SerializeField] [Tooltip("Time taken to accelerate to player movement speed")]
    private float groundAccelerationTime;

    [SerializeField] [Tooltip("Time to transition from walking to sprinting speed")]
    public float sprintTransitionTime;

    [SerializeField]
    private float hitGroundEventThreshold;

    private float moveSpeed;
    private float sprintKeyHeldTime;

    private float gravity;
    private float initialJumpVelocity;

    private bool isJumping;
    private bool isRising;
    private bool isFalling;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    //Events
    public delegate void onHitGround(Vector3 position, float downwardSpeed);
    public event onHitGround OnHitGround;

    public delegate void onJump(Vector3 position, float jumpVelocity);
    public event onJump OnJump;

    private void Awake() {
        statHandler = gameObject.GetComponent<StatHandler>();
    }

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update() {
        CalculateMoveSpeed();
        HandleMovement();
        CalculateGravityValues();
        HandleGravity();
        HandleJump();

        controller.Move(velocity * Time.deltaTime);
    }

    void CalculateMoveSpeed() {
        bool sprintHeld = Input.GetAxisRaw("Sprint") > 0;

        //Fix edge case
        if (sprintTransitionTime == 0) {
            moveSpeed = sprintHeld ? statHandler.movementSpeed * statHandler.sprintMultiplier
                                   : statHandler.movementSpeed;
            return;
        }

        if(sprintHeld) {
            sprintKeyHeldTime += Time.deltaTime;
        } else {
            //Detrasition out of sprint
            sprintKeyHeldTime = Mathf.Clamp(sprintKeyHeldTime, 0, sprintTransitionTime);
            sprintKeyHeldTime -= Time.deltaTime;
            sprintKeyHeldTime = Mathf.Clamp(sprintKeyHeldTime, 0, sprintTransitionTime);
        }

        float speedLerp = Mathf.Clamp01(Mathf.InverseLerp(0, sprintTransitionTime, sprintKeyHeldTime));
        moveSpeed = Mathf.Lerp(statHandler.movementSpeed, statHandler.movementSpeed * statHandler.sprintMultiplier, speedLerp);
    }

    void HandleMovement() {
        Vector2 inputVectorRaw = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")).normalized;
        dampedInput = Vector2.SmoothDamp(dampedInput, inputVectorRaw, ref dampedInputVelocity, groundAccelerationTime);

        Vector3 transformDirection = (transform.forward * dampedInput.x) + (transform.right * dampedInput.y);
        velocity.x = transformDirection.x * moveSpeed;
        velocity.z = transformDirection.z * moveSpeed;
    }

    void CalculateGravityValues() {
        gravity = (-2 * statHandler.jumpHeight) / (Mathf.Pow(timeToApex, 2));
        if (isFalling && !controller.isGrounded) {
            gravity *= gravityWhilstFallingMultiplier;
        }

        //Calculation not nessisarily needed if isFalling as cant jump anyway
        initialJumpVelocity = -gravity * timeToApex;
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
            velocity.y -= -gravity * Time.deltaTime;
            if (velocity.y < -terminalVelocity) velocity.y = -terminalVelocity;
        }
    }

    void HandleJump() {
        if (controller.isGrounded) {
            //Reset jumping trackers
            isJumping = false;
            isFalling = false;
            isRising = false;
        }else if(velocity.y < 0) {
            //Player is falling
            isFalling = true;
            isRising = false;
        }

        bool jumpKeyPressed = Input.GetAxisRaw("Jump") > 0;
        if(controller.isGrounded && jumpKeyPressed && !isJumping) {
            isJumping = true;
            isRising = true;
            velocity.y = initialJumpVelocity;

            if (OnJump != null) OnJump.Invoke(transform.position, initialJumpVelocity);
        }
    }
}
