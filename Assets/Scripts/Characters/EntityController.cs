using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
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
    [SerializeField] private bool isGroundedDisplay; //Editor only

    private float activeDrag = 7f;
    private float activeDragVelocity;
    [SerializeField] private float groundDrag = 7f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float dragTransitionTime = 0.2f;
    [SerializeField] private LayerMask ignoreForGrounded;

    [SerializeField] private float maxStepHeight = 0.5f;
    [SerializeField] private float maxSlopeAngle = 30f;

    [SerializeField] private float minimumMoveDistance = 0.001f;

    [SerializeField] private DebugOptions debugOptions;

    public bool isGrounded { get; private set; }

    [SerializeField] private float skinWidth = 0.005f;
    private CapsuleCollider capsule;
    public float capsuleHeight { get { return Mathf.Max(capsule.radius * 2, capsule.height); } }
    public float capsuleRadius { get { return capsule.radius; } }

    protected virtual void Awake() {
        capsule = GetComponent<CapsuleCollider>();

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
        CheckGrounded();
        if (useGravity) HandleGravity();
        ApplyDrag();

        if (movementProvider)
            HandleMovement(movementProvider.GetMovementState());

        ApplyVelocity();

        //Update editor variables
        velocityDisplay = velocity;
        currentSpeedDisplay = currentSpeed;
        isGroundedDisplay = isGrounded;

        if (behaviourProvider && behaviourProvider.IsActive) {
            behaviourProvider.OnPhysicsUpdate();
            behaviourProvider.OnFrameUpdate();
        }
    }

    private void Jump(float power) {
        if(debugOptions.PrintErrors)
            Debug.Log("Jumped with " + power + " power");

        velocity = new Vector3(velocity.x, power, velocity.z);
    }

    private void HandleGravity() {
        float gravity = 9.81f * 3f;
        AddForce(Vector3.down * gravity, ForceType.Acceleration);
    }

    private void ApplyDrag() {
        float appliedDrag = groundDrag;
        if (!isGrounded)
            appliedDrag = airDrag;

        activeDrag = Mathf.SmoothDamp(activeDrag, appliedDrag, ref activeDragVelocity, dragTransitionTime);

        if (currentSpeed >= 0.05) {
            Vector3 dragForce = new Vector3(-velocity.x, 0f, -velocity.z).normalized * activeDrag;
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
        Vector3 wishTranslation = velocity * Time.deltaTime;
        Move(wishTranslation, out Vector3 calcVelocity);

        //velocity = (transform.position - beforePosition) / Time.deltaTime;
        velocity = calcVelocity;
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

            Vector3 outVelocity;
            Move(climbStep + Vector3.up * skinWidth, out outVelocity);
            velocity = new Vector3(velocity.x, 0f, velocity.z);
            return true;
        }
        return false;
    }

    private bool ClimbSlope(Vector3 normal, out float slopeAngle) {
        if (normal == Vector3.up) {
            slopeAngle = 0;
            return false; //Surface is a floor not a slope!
        }

        slopeAngle = Mathf.Acos(Vector3.Dot(normal, Vector3.up)) * Mathf.Rad2Deg;
        if (slopeAngle - maxSlopeAngle >= 0.01f) {
            return false; //Slope angle is greater than maximum
        }
        if(slopeAngle <= 0.5f) {
            slopeAngle = 0;
            return false;
        }

        if (debugOptions.PrintErrors)
            Debug.Log("Climbing slope of angle " + Mathf.RoundToInt(slopeAngle));

        return true;
    }

    private bool TestMovement(Vector3 translation, out RaycastHit hitInfo) {
        translation += translation.normalized * skinWidth;

        Vector3 center = transform.position + capsule.center;
        float capsulePointHeight = capsuleHeight - capsule.radius * 2f;

        Vector3 point1 = center + (transform.up * capsulePointHeight / 2f);
        Vector3 point2 = center - (transform.up * capsulePointHeight / 2f);

        return Physics.CapsuleCast(point1, point2, capsule.radius, translation.normalized, out hitInfo, translation.magnitude, ~ignoreForGrounded);
    }

    public void Move(Vector3 translation, out Vector3 velocity) {
        velocity = Vector3.zero;
        bool climbedSlope = false;

        for (int i = 0; i < 3; i++) {
            if (translation.magnitude <= minimumMoveDistance) {
                velocity = Vector3.zero;
                return;
            }

            if (TestMovement(translation, out RaycastHit hit)) {
                Vector3 normal = hit.normal;

                if(debugOptions.ShowCollisionPoints)
                    DrawTestSphere(hit.point, 1.0f);

                if (ClimbSlope(hit.normal, out float angle)) {
                    Vector2 flatDir = new Vector2(translation.x, translation.z);
                    float scalar = flatDir.magnitude;
                    float nX = scalar * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float nY = scalar * Mathf.Sin(angle * Mathf.Deg2Rad);
                    Vector2 flatTranslation = flatDir * nX;
                    Vector3 climbTranslation = new Vector3(flatTranslation.x, nY, flatTranslation.y);

                    if (TestMovement(climbTranslation, out hit)) {
                        // If new climb translation results in a collision, treat the slope as a flat wall
                        //normal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                    } else {
                        velocity = new Vector3(translation.x, 0f, translation.z) / Time.deltaTime;
                        translation = climbTranslation;
                        climbedSlope = true;
                        break;
                    }

                } else if(angle >= maxSlopeAngle) {
                    //Better results without this
                    //Hit too steep slope so should treat slope as a wall;
                    //normal = new Vector3(hit.normal.x, -hit.normal.y, hit.normal.z).normalized;
                }

                float dot = Vector3.Dot(normal, translation);
                translation -= normal * dot;
            }
        }

        if(TestMovement(translation, out _)) {
            if(debugOptions.PrintErrors)
                Debug.Log("Collision outlier found");

            velocity = Vector3.zero;
            return;
        }

        transform.position += translation;

        if (!climbedSlope)
            velocity = translation / Time.deltaTime;
    }

    private void CheckGrounded() {
        float scalar = skinWidth * 1.5f;
        isGrounded = TestMovement(Vector3.down * scalar, out RaycastHit _);
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
            case ForceType.Acceleration:
                velocity += force * Time.deltaTime;
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

    private void DrawTestSphere(Vector3 position, float lifetime) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = position;
        go.transform.localScale *= 0.1f;
        go.GetComponent<MeshRenderer>().material.color = Color.red;
        Destroy(go.GetComponent<Collider>());
        StartCoroutine(TestSphereLifetime(go, lifetime));
    }

    private IEnumerator TestSphereLifetime(GameObject sphere, float lifetime) {
        yield return new WaitForSeconds(lifetime);
        Destroy(sphere);
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying)
            return;

        if (debugOptions.ShowCollisionFrame) {
            Vector3 wishTranslation = velocity * Time.deltaTime;
            bool hit = TestMovement(wishTranslation, out _);

            Vector3 center = transform.position + capsule.center;
            float capsulePointHeight = capsuleHeight - capsule.radius * 2f;

            Vector3 point1 = center + (transform.up * capsulePointHeight / 2f);
            Vector3 point2 = center - (transform.up * capsulePointHeight / 2f);

            Gizmos.color = hit ? Color.red : Color.green;

            Gizmos.DrawWireSphere(point1 + wishTranslation, capsuleRadius);
            Gizmos.DrawWireSphere(point2 + wishTranslation, capsuleRadius);
        }
    }

    [System.Serializable]
    struct DebugOptions {
        public bool enabled;
        [SerializeField]
        private bool showCollisionsPoints;
        public bool ShowCollisionPoints { get { return enabled && showCollisionsPoints; } }
        [SerializeField]
        private bool printErrors;
        public bool PrintErrors { get { return enabled && printErrors; } }
        [SerializeField]
        private bool showCollisionFrame;
        public bool ShowCollisionFrame { get { return enabled && showCollisionFrame; } }
    }

}

public enum ForceType {
    Force,
    Impulse,
    Acceleration
}