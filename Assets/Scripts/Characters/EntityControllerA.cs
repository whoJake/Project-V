/*
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EntityController : MonoBehaviour {
    [SerializeField] protected MovementProvider movementProvider;
    public MovementProvider MovementProvider { get { return movementProvider; } }

    [SerializeField] protected BehaviourProvider behaviourProvider;
    public BehaviourProvider BehaviourProvider { get { return behaviourProvider; } }

    [SerializeField] private float mass = 1f;
    [SerializeField] public bool useGravity = true;

    public GameObject lockonTarget;

    public Vector3 velocity { get; private set; }
    public Vector2 v2Velocity { get { return new Vector2(velocity.x, velocity.z); } }
    public float currentSpeed { get { return v2Velocity.magnitude; } }

    [SerializeField] private float currentSpeedDisplay; //Editor only
    [SerializeField] private Vector3 velocityDisplay; //Editor only

    [SerializeField] private float groundDrag = 7f;
    [SerializeField] private float airDrag = 2f;

    public bool isGrounded { get { return characterController.isGrounded; } }

    public float capsuleHeight { get { return Mathf.Max(characterController.radius * 2, characterController.height); } }
    public float capsuleRadius { get { return characterController.radius; } }

    private CharacterController characterController;

    protected virtual void Awake() {
        characterController = GetComponent<CharacterController>();

        if (movementProvider) {
            SetMovementProvider(movementProvider);
            movementProvider.Initialize(this);
        }

        if (behaviourProvider) {
            SetBehaviourProvider(behaviourProvider);
            behaviourProvider.Initialize(this);
        }
    }

    protected virtual void Update() {
        if (useGravity) HandleGravity();
        ApplyDrag();

        if (movementProvider)
            HandleMovement(movementProvider.GetMovementState());

        ApplyVelocity();

        //Update editor variables
        velocityDisplay = velocity;
        currentSpeedDisplay = currentSpeed;

        if (behaviourProvider && behaviourProvider.IsActive) {
            behaviourProvider.OnPhysicsUpdate();
            behaviourProvider.OnFrameUpdate();
        }
    }

    private void Jump(float power) {
        Debug.Log("Jumped with " + power + " power");
        velocity = new Vector3(velocity.x, power, velocity.z);
    }

    private void HandleGravity() {
        AddForce(Vector3.down * 9.81f, ForceType.Force);
    }

    private void ApplyDrag() {
        float appliedDrag = groundDrag;
        if (!isGrounded)
            appliedDrag = airDrag;

        if (currentSpeed >= 0.05) {
            Vector3 dragForce = new Vector3(-velocity.x, 0f, -velocity.z).normalized * appliedDrag;
            AddForce(dragForce, ForceType.Force);
        } else {
            velocity = new Vector3(0f, velocity.y, 0f);
        }
    }

    private void HandleMovement(MovementState state) {
        Vector3 moveForce = (transform.forward * state.direction.y + transform.right * state.direction.x) * state.speed;
        AddForce(moveForce, ForceType.Force);

        Vector3 clampedMoveVelocity = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z), state.speed);
        velocity = new Vector3(clampedMoveVelocity.x, velocity.y, clampedMoveVelocity.z);
    }

    private void ApplyVelocity() {
        Vector3 desiredTranslation = velocity * Time.deltaTime;
        characterController.Move(desiredTranslation);
        float ydiff = characterController.velocity.y - velocity.y;
        float prevy = velocity.y;
        velocity = characterController.velocity;
        ydiff = Mathf.Min(ydiff, 5f);
        velocity = new Vector3(velocity.x, prevy + ydiff, velocity.z);
    }

    public void AddForce(Vector3 force, ForceType type) {
        switch (type) {
            case ForceType.Force:
                Vector3 acceleration = force / mass;
                velocity += Time.deltaTime * 3f * acceleration;
                break;
            case ForceType.Impulse:
                velocity += force;
                break;
        }
    }

    public void SetMovementProvider(MovementProvider a) {
        if (movementProvider) movementProvider.OnJump -= Jump;
        movementProvider = Instantiate(a);
        movementProvider.OnJump += Jump;
    }

    public void SetBehaviourProvider(BehaviourProvider a) {
        behaviourProvider = Instantiate(a);
    }
}

public enum ForceType {
    Force,
    Impulse
}
*/