using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Behaviour Providers/Shoot In Range")]
public class ShootInRangeBehaviour : BehaviourProvider
{
    [SerializeField] private float range;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackTime;

    private Coroutine fireSequence;

    public override void OnFrameUpdate() {
        if (controller.lockonTarget) {
            if(Vector3.Distance(controller.transform.position, controller.lockonTarget.transform.position) < range) {
                Enable();
            } else {
                Disable();
            }
        } else {
            Disable();
        }
    }

    private IEnumerator FireSequence() {
        while (true) {
            yield return new WaitForSeconds(1f / attackSpeed);

            if(controller.MovementProvider.canOverride == true) 
                controller.StartCoroutine(Fire());
        }
    }

    private IEnumerator Fire() {
        controller.MovementProvider.canMove = false;
        yield return new WaitForSeconds(attackTime / 2f);
        SpawnBullet();
        yield return new WaitForSeconds(attackTime / 2f);
        controller.MovementProvider.canMove = true;
    }

    private void SpawnBullet() {
        GameObject bullet = GameObject.Instantiate(bulletPrefab);
        Vector3 vec2target = (controller.lockonTarget.transform.position - controller.transform.position).normalized;
        bullet.transform.position = controller.transform.position + vec2target * 1f;
        bullet.transform.forward = vec2target;

        bullet.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
    }

    public override void Enable() {
        base.Enable();

        if (fireSequence == null) {
            fireSequence = controller.StartCoroutine(FireSequence());
            controller.GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    public override void Disable() {
        base.Disable();

        if (fireSequence != null) {
            controller.StopCoroutine(fireSequence);
            controller.GetComponent<MeshRenderer>().material.color = Color.white;
        }

        fireSequence = null;
    }

}
