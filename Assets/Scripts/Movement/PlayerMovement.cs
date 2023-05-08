using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private StatHandler statHandler;

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


    [SerializeField] //For display not editing
    private Vector3 velocity;
    private Vector2 v2_velocity { get { return new Vector2(velocity.x, velocity.z); } }

    private float moveSpeed;
    private float sprintKeyHeldTime;

    private float gravity;
    private float initialJumpVelocity;

    private bool hasInput;
    private bool isMoving;
    private bool isJumping;
    private bool isRising;
    private bool isFalling;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    [SerializeField]
    private float slowDownDrag;
    [SerializeField]
    private float airDrag;
    [SerializeField]
    private float dragTransitionTime;
    private float currentDrag;
    private float dampedDragVelocity;

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
        CalculateDrag();
        HandleDrag();

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
        hasInput = inputVectorRaw.magnitude != 0;
        dampedInput = Vector2.SmoothDamp(dampedInput, inputVectorRaw, ref dampedInputVelocity, groundAccelerationTime);

        Vector3 desiredMove = ((transform.forward * dampedInput.x) + (transform.right * dampedInput.y));
        Vector2 horizontalVelocity = new Vector2(velocity.x + desiredMove.x, velocity.z + desiredMove.z);
        horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, moveSpeed);

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.y;

        isMoving = horizontalVelocity.magnitude >= 0.05f;
    }

    void CalculateDrag() {
        float targetDrag;
        if (controller.isGrounded && hasInput) {
            targetDrag = 0.5f;
        } else if (controller.isGrounded && !hasInput && isMoving) {
            targetDrag = slowDownDrag;
        } else {
            targetDrag = airDrag;
        }

        currentDrag = Mathf.SmoothDamp(currentDrag, targetDrag, ref dampedDragVelocity, dragTransitionTime);

    }

    void HandleDrag() {
        Vector2 horizontalVelocity = Vector2.ClampMagnitude(v2_velocity, moveSpeed);

        float speed = horizontalVelocity.magnitude;

        float dragCoefficient = 2 / (moveSpeed * moveSpeed);

        float dragForce = currentDrag * Mathf.Pow(speed, 2) * dragCoefficient;

        //                                                    Ensure theres no overcompensation
        horizontalVelocity -= horizontalVelocity.normalized * Mathf.Min(speed, dragForce);
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.y;
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
