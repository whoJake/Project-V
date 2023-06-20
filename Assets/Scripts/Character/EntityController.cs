using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class EntityController : MonoBehaviour
{
    [SerializeField] private MovementProvider movementProvider;
    [SerializeField] private BehaviourProvider behaviourProvider;

    [SerializeField] private float mass = 1f;
    [SerializeField] private bool useGravity = true;
    public Vector3 velocity;
    [SerializeField] private float drag;
    [SerializeField] private LayerMask ignoreForGrounded;

    public float currentSpeed;

    public bool isGrounded { get; private set; }

    [SerializeField] private float skinWidth = 0.001f;
    private CapsuleCollider capsule;

    private void Awake() {
        capsule = GetComponent<CapsuleCollider>();

        movementProvider.Initialize(gameObject);
        behaviourProvider.Initialize(gameObject);

        movementProvider.OnJump += Jump;
    }

    private void Jump(float power) {
        Debug.Log("Jumped");
        AddForce(Vector3.up * power, ForceType.Impulse);
    }

    private void HandleGravity() {
        if (isGrounded) {
            velocity = new Vector3(velocity.x, 0f, velocity.z);
        } else {
            AddForce(Vector3.down * 9.81f, ForceType.Force);
        }
    }

    public void AddForce(Vector3 force, ForceType type) {
        switch (type) {
            case ForceType.Force:
                Vector3 acceleration = force / mass;
                velocity += acceleration * Time.deltaTime * 3;
                break;
            case ForceType.Impulse:
                velocity += force;
                break;
        }
    }

    private void ApplyVelocity() {
        Vector3 desiredTranslation = velocity * Time.deltaTime;
        Move(desiredTranslation);
    }

    private bool TryMovement(Vector3 translation, out RaycastHit hitInfo) {
        Vector3 center = transform.position + capsule.center;
        float capsulePointHeight = Mathf.Max(0f, capsule.height - (capsule.radius * 2));

        Vector3 point1 = center + ( transform.up * capsulePointHeight / 2f );
        Vector3 point2 = center - ( transform.up * capsulePointHeight / 2f);

        return Physics.CapsuleCast(point1, point2, capsule.radius - skinWidth, translation.normalized, out hitInfo, translation.magnitude);
    }

    public void Move(Vector3 translation) {
        bool failed = TryMovement(translation, out RaycastHit hit);
        Debug.DrawLine(transform.position, transform.position + translation, Color.blue, 2);

        if (failed) {
            translation = translation.normalized * (hit.distance - skinWidth);
        }

        Debug.DrawLine(transform.position, transform.position + translation, Color.red, 2);
        transform.position += translation;
    }

    private void HandleMovement(MovementState state) {
        Vector3 moveForce = (transform.forward * state.direction.y + transform.right * state.direction.x) * state.speed;
        AddForce(moveForce, ForceType.Force);

        Vector3 clampedMoveVelocity = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z), state.speed);
        velocity = new Vector3(clampedMoveVelocity.x, velocity.y, clampedMoveVelocity.z);
    }

    private void ApplyDrag() {
        float appliedDrag = drag;
        if (!isGrounded)
            appliedDrag = 0f;

        if (currentSpeed >= 0.05) {
            Vector3 dragForce = new Vector3(-velocity.x, 0f, -velocity.z).normalized * appliedDrag;
            AddForce(dragForce, ForceType.Force);
        } else {
            velocity = new Vector3(0f, velocity.y, 0f);
        }
    }

    private void CheckGrounded() {
        isGrounded = TryMovement(Vector3.down * skinWidth, out RaycastHit _);
    }

    private void Update() {
        CheckGrounded();
        if (useGravity) HandleGravity();
        ApplyDrag();

        HandleMovement(movementProvider.GetMovementState());
        ApplyVelocity();
        currentSpeed = new Vector2(velocity.x, velocity.z).magnitude;

        behaviourProvider.OnFrameUpdate();
    }
}

public enum ForceType {
    Force,
    Impulse
}
