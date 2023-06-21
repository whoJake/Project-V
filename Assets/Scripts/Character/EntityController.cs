using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class EntityController : MonoBehaviour
{
    [SerializeField] private MovementProvider movementProvider;
    [SerializeField] private BehaviourProvider behaviourProvider;

    [SerializeField] private float mass = 1f;
    [SerializeField] private bool useGravity = true;
    public Vector3 velocity;
    [SerializeField] private float drag = 7f;
    [SerializeField] private LayerMask ignoreForGrounded;

    public float currentSpeed;
    [SerializeField] private int maxCollisionChecks = 5;
    [SerializeField] private float minimumMoveDistance = 0.001f;

    public bool isGrounded;

    [SerializeField] private float skinWidth = 0.005f;
    private CapsuleCollider capsule;

    private void Awake() {
        capsule = GetComponent<CapsuleCollider>();

        movementProvider.Initialize(this);
        behaviourProvider.Initialize(this);

        movementProvider.OnJump += Jump;
    }

    private void Jump(float power) {
        Debug.Log("Jumped");
        velocity = new Vector3(velocity.x, power, velocity.z);
        //AddForce(Vector3.up * power, ForceType.Impulse);
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
                velocity += Time.deltaTime * 3f * acceleration;
                break;
            case ForceType.Impulse:
                velocity += force;
                break;
        }
    }

    private void ApplyVelocity() {
        Vector3 desiredTranslation = velocity * Time.deltaTime;

        //Still causes issues specifically under 30 degree decline roofs but seems to work well enough for now
        int checks = 0;

        //Remove components of velocity that move it towards a collision, try new velocity and repeat
        while (TestMovement(desiredTranslation, out RaycastHit hit) && checks < maxCollisionChecks) {
            float dot = Vector3.Dot(hit.normal, velocity);
            velocity -= hit.normal * dot;
            desiredTranslation = velocity * Time.deltaTime;
            checks++;
        }
        if (checks == maxCollisionChecks)
            velocity = Vector3.zero;

        Move(desiredTranslation);
    }
    

    private bool TestMovement(Vector3 translation, out RaycastHit hitInfo) {
        translation += translation.normalized * skinWidth;

        Vector3 center = transform.position + capsule.center;
        float capsulePointHeight = Mathf.Max(0f, capsule.height - (capsule.radius * 2));

        Vector3 point1 = center + ( transform.up * capsulePointHeight / 2f );
        Vector3 point2 = center - ( transform.up * capsulePointHeight / 2f );

        return Physics.CapsuleCast(point1, point2, capsule.radius, translation.normalized, out hitInfo, translation.magnitude);
    }

    public void Move(Vector3 translation) {
        bool failed = TestMovement(translation, out RaycastHit hit);

        if (failed)
            translation = translation.normalized * (hit.distance - skinWidth);

        if (translation.magnitude <= minimumMoveDistance)
            translation = Vector3.zero;

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
        isGrounded = TestMovement(Vector3.down * skinWidth * 1.01f, out RaycastHit _);
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
