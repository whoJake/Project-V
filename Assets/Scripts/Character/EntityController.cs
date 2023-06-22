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

    public Vector3 velocity { get; private set; }
    public Vector2 v2Velocity { get { return new Vector2(velocity.x, velocity.z); } }
    public float currentSpeed { get { return v2Velocity.magnitude; } }

    [SerializeField] private float currentSpeedDisplay; //Editor only
    [SerializeField] private Vector3 velocityDisplay; //Editor only

    [SerializeField] private float groundDrag = 7f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private LayerMask ignoreForGrounded;

    [SerializeField] private float maxStepHeight = 0.5f;
    [SerializeField] private float maxSlopeAngle = 30f;

    [SerializeField] private int maxCollisionChecks = 5;
    [SerializeField] private float minimumMoveDistance = 0.001f;

    public bool isGrounded { get; private set; }

    [SerializeField] private float skinWidth = 0.005f;
    private CapsuleCollider capsule;
    private float capsuleHeight { get { return Mathf.Max(capsule.radius * 2, capsule.height); } }

    private void Awake() {
        capsule = GetComponent<CapsuleCollider>();

        movementProvider.Initialize(this);
        behaviourProvider.Initialize(this);

        movementProvider.OnJump += Jump;
    }

    private void Update() {
        CheckGrounded();
        if (useGravity) HandleGravity();
        ApplyDrag();

        HandleMovement(movementProvider.GetMovementState());
        ApplyVelocity();

        //Update editor variables
        velocityDisplay = velocity;
        currentSpeedDisplay = currentSpeed;

        behaviourProvider.OnFrameUpdate();
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

        //Still causes issues specifically under 30 degree decline roofs but seems to work well enough for now
        int checks = 0;

        //Remove components of velocity that move it towards a collision, try new velocity and repeat
        while (TestMovement(desiredTranslation, out RaycastHit hit) && checks < maxCollisionChecks) {
            
            checks++;
            if (Vector3.Angle(hit.normal, Vector3.up) - maxSlopeAngle <= 0.001f)
                break; //Hit slope, velocity stays the same

            if (TryClimbStep(hit.point)) {
                desiredTranslation = velocity * Time.deltaTime;
                continue;
            }

            //Applies for collisions
            float dot = Vector3.Dot(hit.normal, velocity);
            velocity -= hit.normal * dot;
            desiredTranslation = velocity * Time.deltaTime;
        }

        if (checks == maxCollisionChecks)
            velocity = Vector3.zero;

        Move(desiredTranslation);
    }

    private bool TryClimbStep(Vector3 hitPoint) {
        float transformBottomY = transform.position.y - capsuleHeight / 2f;
        Vector3 origin = new Vector3(hitPoint.x, transformBottomY, hitPoint.z);
        origin.y += maxStepHeight + skinWidth;
        origin += new Vector3(velocity.x, 0f, velocity.z).normalized * skinWidth;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxStepHeight + skinWidth, ~ignoreForGrounded)) {
            if (Physics.Raycast(hit.point, Vector3.up, out RaycastHit _, capsuleHeight + skinWidth * 4, ~ignoreForGrounded))
                return false; //Gap above stair isnt large enough to fit the capusule

            if (Vector3.Dot(hit.normal, Vector3.up) <= 0.99)
                return false; //Detected surface is not a step

            Vector3 climbStep = new Vector3(0f, hit.point.y - transformBottomY, 0f);
            if (TestMovement(climbStep, out RaycastHit _))
                return false; //Cannot move up enough to climb step and not hit head

            Move(climbStep + Vector3.up * skinWidth);
            velocity = new Vector3(velocity.x, 0f, velocity.z);
            return true;
        }
        return false;
    }

    private bool TryClimbSlope(ref Vector3 translation, Vector3 normal) {
        if (normal == Vector3.up) {
            return false; //Surface is a floor not a slope!
        }

        float slopeAngle = Mathf.Acos(Vector3.Dot(normal, Vector3.up)) * Mathf.Rad2Deg;
        if (slopeAngle - maxSlopeAngle >= 0.01f) {
            return false; //Slope angle is greater than maximum
        }

        float targetDst = new Vector2(translation.x, translation.z).magnitude;
        float newX = targetDst * Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
        float newY = targetDst * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        translation = new Vector3(translation.x, 0f, translation.z).normalized * newX + Vector3.up * newY;
        return true;
    }

    private bool TestMovement(Vector3 translation, out RaycastHit hitInfo) {
        translation += translation.normalized * skinWidth;

        Vector3 center = transform.position + capsule.center;
        float capsulePointHeight = capsuleHeight - capsule.radius * 2f;

        Vector3 point1 = center + ( transform.up * capsulePointHeight / 2f );
        Vector3 point2 = center - ( transform.up * capsulePointHeight / 2f );

        return Physics.CapsuleCast(point1, point2, capsule.radius, translation.normalized, out hitInfo, translation.magnitude);
    }

    public void Move(Vector3 translation) {
        //bool failed = TestMovement(translation, out RaycastHit hit);
        int count = 0;

        while(TestMovement(translation, out RaycastHit hit) && count < maxCollisionChecks) {
            count++;
            if (translation.magnitude <= minimumMoveDistance)
                translation = Vector3.zero;

            if (TryClimbSlope(ref translation, hit.normal))
                continue;

            translation = translation.normalized * (hit.distance - skinWidth);
        }

        if (count == maxCollisionChecks)
            translation = Vector3.zero;

        transform.position += translation;
    }

    private void CheckGrounded() {
        isGrounded = TestMovement(Vector3.down * skinWidth * 1.01f, out RaycastHit _);
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

}

public enum ForceType {
    Force,
    Impulse
}
