using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
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
    private float sprintTransitionTime;

    [SerializeField] [Tooltip("This value is multiplied by groundAccelerationTime when airbourne in order to slowdown acceleration/deceleration when airbourne")]
    private float airbourneMovementPenalty;

    [SerializeField]
    private float hitGroundEventThreshold;

    [SerializeField] //For display not editing
    private Vector3 velocity;

    [SerializeField]
    private bool endlessJump;

    private float moveSpeed;
    private float sprintKeyHeldTime;

    private float gravity;
    private float initialJumpVelocity;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    private bool hasInput;
    private bool isMoving;
    private bool isAirbourne;
    private bool isJumping;
    private bool isRising;
    private bool isFalling;

    [SerializeField]
    private bool disableBunnyhopping;
    private bool canJump;

    public bool isActive;

    //Events
    public Action<Vector3, float> OnHitGround;
    public Action<Vector3, float> OnJump;

    private void Awake() {
        statHandler = gameObject.GetComponent<StatHandler>();
        TerrainHandler.OnLayerGenerated += OnLayerGenerated;
    }

    private void OnDestroy() {
        TerrainHandler.OnLayerGenerated -= OnLayerGenerated;
    }

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update() {
        if (!isActive) return;

        UpdateState();

        CalculateMoveSpeed();
        HandleMovement();

        CalculateGravityValues();
        HandleGravity();
        HandleJump();

        controller.Move(velocity * Time.deltaTime);
    }

    //
    // Summery:
    //     Updates most of the isX values to ensure the player is in the right
    //     state to make other calculations
    //
    void UpdateState() {
        isAirbourne = !controller.isGrounded;

        if (controller.isGrounded) {
            //Call OnHitGround before resetting if was falling frame before
            if (isFalling && velocity.y <= -hitGroundEventThreshold) OnHitGround?.Invoke(transform.position, velocity.y);

            //Reset jumping trackers
            isJumping = false;
            isFalling = false;
            isRising = false;
        } else if (velocity.y < 0) {
            //Player is falling
            isFalling = true;
            isRising = false;
        }
    }

    //
    // Summery:
    //     Calculates the desired movespeed of the player by taking into account
    //     weather they are sprinting or not and damping between those two values
    //     if necissary
    //
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

    //
    // Summery:
    //     Handles the player controlled movement by taking the raw input, damping
    //     it to produce smoother movement transitions and then applying that movement
    //     to the player controller.
    //     Movement speed is also clamped in this method
    //
    void HandleMovement() {
        Vector2 inputVectorRaw = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")).normalized;
        hasInput = inputVectorRaw.magnitude != 0;

        //Takes longer to accelerate/decelerate when airbourne
        float dampTime = groundAccelerationTime;
        if (isAirbourne) dampTime *= airbourneMovementPenalty;

        //Damp the input
        dampedInput = Vector2.SmoothDamp(dampedInput, inputVectorRaw, ref dampedInputVelocity, dampTime);

        Vector3 desiredMove = (transform.forward * dampedInput.x) + (transform.right * dampedInput.y);

        velocity.x = desiredMove.x * moveSpeed;
        velocity.z = desiredMove.z * moveSpeed;

        Vector2 v2_velocity = new Vector2(velocity.x, velocity.z);
        isMoving = v2_velocity.magnitude >= 0.05f;
    }

    //
    // Summery:
    //     Calculates values relating to gravity so that it can then be applied
    //     to the player. Gravity is calculated based on the given JumpHeight
    //     and TimeToApex instead of being a hardcoded value
    //
    void CalculateGravityValues() {
        //Standard jump equation derived by https://www.youtube.com/watch?v=hG9SzQxaCm8 [GDC Math for Game Programmers: Building a Better Jump] a classic
        gravity = (-2 * statHandler.jumpHeight) / (Mathf.Pow(timeToApex, 2));
        if (isFalling && !controller.isGrounded) {
            gravity *= gravityWhilstFallingMultiplier;
        }

        //Calculation not nessisarily needed if isFalling as cant jump anyway
        initialJumpVelocity = -gravity * timeToApex;
    }

    //
    // Summery:
    //     Applies the previously calculated gravity to the player and
    //     handles some edge cases to do with the player colliding with
    //     an object whilst in mid air
    //
    void HandleGravity() {
        //This accounts for hitting head
        if(isAirbourne && controller.velocity.y == 0) {
            velocity.y = 0;
        }

        if (controller.isGrounded) {
            velocity.y = -0.1f;
        } else {
            velocity.y -= -gravity * Time.deltaTime;
            if (velocity.y < -terminalVelocity) velocity.y = -terminalVelocity;
        }
    }

    //
    // Summery:
    //     Detects and executes a jump when the player holds down the jump
    //     key. Also sets state variables to reflect this
    //
    void HandleJump() {
        bool jumpKeyPressed = Input.GetAxisRaw("Jump") > 0;
        if (!jumpKeyPressed && !isJumping) canJump = true; //This is to effectively disables bunnyhopping
                                                           //No cooldown to jumping so can still technically jump on first frame and only experience one frame of floor drag

        if((controller.isGrounded && jumpKeyPressed && !isJumping) || (endlessJump && jumpKeyPressed)) {
            if (disableBunnyhopping && !canJump) return;
            canJump = false;
            isJumping = true;
            isRising = true;
            velocity.y = initialJumpVelocity;

            OnJump?.Invoke(transform.position, initialJumpVelocity);
        }
    }

    //
    // Summery:
    //     Will activate this controller once the first layer has fully generated
    //
    private void OnLayerGenerated(int index) {
        if (index == 1) isActive = true;
    }

}
