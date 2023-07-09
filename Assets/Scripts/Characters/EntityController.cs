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

    private CharacterController characterController;

    private float gravity;
    [SerializeField] private float timeToReachApex = 0.2f;
    [SerializeField] public bool useGravity = true;

    [SerializeField] private float velocitySmoothTime;
    private float xMul, yMul;
    private float xVel, yVel;
    private float verticalVelocity = 0f; //Have to handle this seperately

    public GameObject lockonTarget;

    [SerializeField] private Vector2 velocitySmoothingDisplay; //Editor only
    [SerializeField] private bool isGroundedDisplay; //Editor only

    public bool isGrounded { get; private set; }

    public float capsuleHeight { get { return Mathf.Max(characterController.radius * 2, characterController.height); } }
    public float capsuleRadius { get { return characterController.radius; } }


    protected virtual void Awake() {
        characterController = GetComponent<CharacterController>();
        //Begin gravity value
        gravity = 2 * 10 / (Mathf.Pow(timeToReachApex, 2));

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
        velocitySmoothingDisplay = new Vector2(xMul, yMul);
        isGroundedDisplay = isGrounded;

        if (useGravity) HandleGravity();

        if (movementProvider)
            HandleMovement(movementProvider.GetMovementState());

        if (behaviourProvider && behaviourProvider.IsActive) {
            behaviourProvider.OnPhysicsUpdate();
            behaviourProvider.OnFrameUpdate();
        }
    }

    private void Jump(float jumpHeight) {
        Debug.Log("Jumped with " + jumpHeight + " jumpHeight");
        gravity = 2 * jumpHeight / (Mathf.Pow(timeToReachApex, 2));
        verticalVelocity = gravity * timeToReachApex;
    }

    private void HandleGravity() {
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        isGrounded = characterController.isGrounded;

        if (isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity = Mathf.Max(-gravity * 10, verticalVelocity - gravity * Time.deltaTime);
    }

    private void HandleMovement(MovementState state) {
        xMul = Mathf.SmoothDamp(xMul, state.direction.x, ref xVel, velocitySmoothTime);
        yMul = Mathf.SmoothDamp(yMul, state.direction.y, ref yVel, velocitySmoothTime);

        Vector2 direction = new Vector2(xMul, yMul);

        Vector3 dirMove = (transform.forward * direction.y + transform.right * direction.x) * state.speed;
        characterController.Move(dirMove * Time.deltaTime);
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
