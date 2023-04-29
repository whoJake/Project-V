using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Max speed the body can reach through controlled movement")]
    [Min(0)] public float speed;
    [Tooltip("How fast the body will reach max speed and decelerate back to standing")]
    [Min(0)] public float accelerationTime;
    [Tooltip("The effect of gravity on this body")]
    [Min(0)] public float gravity;
    [Min(0)] public float maxFallSpeed;

    [Min(0)] public float jumpHeight;

    private Vector3 movementVelocity;
    public Vector3 gravitationalVelocity;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    public bool isGrounded;
    public bool isJumping;
    public Vector3 jumpVelocity;

    private CharacterController controller;

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    //Jump cooldown
    //Refactor code
    //Fix gravity and jump velocity interaction a bit
    //Update PlayerMovementEditor

    void Jump() {
        isJumping = true;
        jumpVelocity = new Vector3(0, jumpHeight * gravity, 0);
    }

    void Update()
    {
        CollisionFlags flags = controller.collisionFlags;
        isGrounded = flags.HasFlag(CollisionFlags.CollidedBelow);


        if (isGrounded) {
            isJumping = false;
            jumpVelocity = Vector3.zero;
        }
        if (isGrounded && Input.GetAxisRaw("Jump") > 0) Jump();
        if (isJumping) {
            jumpVelocity = new Vector3(0, Mathf.Clamp(jumpVelocity.y - (gravity * 9.81f * Time.deltaTime), 0, float.PositiveInfinity), 0);
        }

        //Velocity due to directional input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        dampedInput = Vector2.SmoothDamp(dampedInput, input, ref dampedInputVelocity, accelerationTime);

        movementVelocity = (transform.forward * -dampedInput.x * speed) +
                           (transform.right * dampedInput.y * speed);


        if (controller.isGrounded) {
            gravitationalVelocity = Vector3.zero;
        } else {
            gravitationalVelocity -= new Vector3(0, gravity * 9.81f * Time.deltaTime, 0);
            gravitationalVelocity = Vector3.ClampMagnitude(gravitationalVelocity, maxFallSpeed * gravity);
        }

        controller.Move((movementVelocity + gravitationalVelocity + jumpVelocity) * Time.deltaTime);
    }
}
