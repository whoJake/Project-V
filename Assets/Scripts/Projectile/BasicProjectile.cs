using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime;
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float destructionRadius;
    [SerializeField] private bool destroyOnCollision;

    private void OnEnable() {
        StartCoroutine(HandleLifetime());
    }

    private IEnumerator HandleLifetime() {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void Update() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    private void MakeHole(Vector3 position) {
        GameObject.FindGameObjectWithTag("MainTerrainHandler").GetComponent<TerrainHandler>().DistributeEditRequest(new ChunkEditRequest(new ChunkPointEdit(position, destructionRadius, false)));
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("IgnoreProjectile"))
            return;

        if (other.CompareTag("Terrain"))
            MakeHole(transform.position);
        
        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable component)) 
            component.Damage(damage);
        
        if (destroyOnCollision) 
            Destroy(gameObject);
    }

}
