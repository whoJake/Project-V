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

    private Vector3 movementVelocity;
    private Vector3 gravitationalVelocity;

    private Vector2 dampedInput;
    private Vector2 dampedInputVelocity;

    private CharacterController controller;

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    //Stick to ground when grounded
    //Jumping
    //Camera Controller

    void Update()
    {
        //Velocity due to directional input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        dampedInput = Vector2.SmoothDamp(dampedInput, input, ref dampedInputVelocity, accelerationTime);

        movementVelocity = new Vector3(dampedInput.x * speed, 0, dampedInput.y * speed);
        gravitationalVelocity = new Vector3(0, -gravity * 9.81f, 0);

        controller.Move((movementVelocity + gravitationalVelocity) * Time.deltaTime);
    }
}
