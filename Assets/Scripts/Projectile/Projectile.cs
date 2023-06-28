using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool useGravity;
    public bool setGravity;
    public float gravity = 9.81f;

    public float speed;
    public float heightOfApex;
    public float timeToReachApex;

    [SerializeField] private float lifetime;
    [SerializeField] private float damage;
    [SerializeField] private float destructionRadius;
    [SerializeField] private bool destroyOnCollision;

    public Vector3 target;

    [HideInInspector] public float verticalVelocity;
    [HideInInspector] public float horizontalVelocity;

    private void OnEnable() {
        StartCoroutine(HandleLifetime());
    }

    private void Update() {
        Vector3 translation = transform.forward * horizontalVelocity
                            + transform.up * verticalVelocity;
        transform.Translate(translation * Time.deltaTime, Space.World);
        verticalVelocity -= gravity * Time.deltaTime;

        //Debug.DrawLine(prevPos, transform.position, Color.red, 5f);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("IgnoreProjectile") || other.CompareTag("Player"))
            return;

        if (other.CompareTag("Terrain"))
            MakeHole(transform.position);

        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable component))
            component.Damage(damage);

        if (destroyOnCollision)
            Destroy(gameObject);
    }

    private IEnumerator HandleLifetime() {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void MakeHole(Vector3 position) {
        GameObject.FindGameObjectWithTag("MainTerrainHandler").GetComponent<TerrainHandler>().DistributeEditRequest(new ChunkEditRequest(new ChunkPointEdit(position, destructionRadius, false)));
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Vector3 targetPosition) {    
        GameObject projectile = Instantiate(prefab);
        projectile.tag = "IgnoreProjectile";
        projectile.layer = LayerMask.NameToLayer("Projectile");
        projectile.transform.position = position;
        projectile.transform.LookAt(targetPosition);
        Projectile script = projectile.GetComponent<Projectile>();

        if (script.useGravity) {
            Vector3 targetModified = new Vector3(targetPosition.x, position.y, targetPosition.z);
            projectile.transform.LookAt(targetModified);

            if(!script.setGravity) 
                script.gravity = (2 * script.heightOfApex) / (script.timeToReachApex * script.timeToReachApex);

            script.verticalVelocity = script.timeToReachApex * script.gravity;
            float verticalDistance = targetPosition.y - position.y;

            //--QUADRATIC SOLVE FOR TIME--
            float a = (-0.5f) * script.gravity;
            float b = script.verticalVelocity;
            float c = -verticalDistance;

            float discriminant = Mathf.Sqrt(b * b - 4 * a * c);
            float timeToHitTarget = (-b - discriminant) / (2 * a);

            float horizontalDistance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(targetPosition.x, targetPosition.z));
            script.horizontalVelocity = horizontalDistance / timeToHitTarget;

        } else {
            script.verticalVelocity = 0f;
            script.horizontalVelocity = script.speed;
            script.gravity = 0f;
        }

        return projectile;
    }

}
