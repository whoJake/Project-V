using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Behaviour Providers/Spitter")]
public class SpitterBehaviour : BehaviourProvider {

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletFireOffset;

    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackWindupTime;
    [SerializeField] private float attackWinddownTime;

    [SerializeField] private int numOfBulletsPerShot;
    [SerializeField] private float bulletSpread;

    [SerializeField] private float detectPlayerRange;
    [SerializeField] private float detectPlayerRangeBuffer;

    private Coroutine shootCoroutine;

    private bool SearchForPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
            return false;

        GameObject closest = null;
        float closestDst = float.PositiveInfinity;
        foreach(GameObject check in players) {
            float dst = Vector3.Distance(controller.transform.position, check.transform.position);
            if(dst < closestDst) {
                closest = check;
                closestDst = dst;
            }  
        }

        if (closestDst < detectPlayerRange) {
            controller.lockonTarget = closest;
            return true;
        }
        return false;
    }

    private void ChangeColor() {
        Material mat = controller.GetComponent<MeshRenderer>().material;
        if (controller.lockonTarget) {
            mat.color = Color.red;
        } else {
            mat.color = Color.white;
        }
    }

    private void StartShooting() {
        if (shootCoroutine == null) {
            if(controller.MovementProvider && controller.MovementProvider.canOverride)
                shootCoroutine = controller.StartCoroutine(ShootingLoop());
        }
    }

    private void StopShooting() {
        if (shootCoroutine != null) {
            controller.StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
    }

    private void Shoot() {
        for(int i = 0; i < numOfBulletsPerShot; i++) {
            float angOnCircle = Random.Range(0f, Mathf.PI * 2);
            float radius = Random.Range(0f, bulletSpread);
            float xSpread = Mathf.Sin(angOnCircle) * radius;
            float ySpread = Mathf.Cos(angOnCircle) * radius;

            Vector3 bulletTarget = controller.lockonTarget.transform.position + Vector3.right * xSpread + Vector3.forward * ySpread;
            GameObject bullet = Projectile.Spawn(bulletPrefab, controller.transform.position + controller.transform.TransformDirection(bulletFireOffset), bulletTarget);

            bullet.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
        }
    }

    private IEnumerator ShootingLoop() {
        while (true) {
            if (controller.MovementProvider && !controller.MovementProvider.canOverride) {
                shootCoroutine = null;
                yield break;
            }

            controller.MovementProvider.canMove = false;
            yield return new WaitForSeconds(attackWindupTime);

            Shoot();

            yield return new WaitForSeconds(attackWinddownTime);
            controller.MovementProvider.canMove = true;

            yield return new WaitForSeconds(1.0f / attackSpeed);
        }
    }

    public override void OnFrameUpdate() {
        ChangeColor();
        if (!controller.lockonTarget && !SearchForPlayer()) {
            StopShooting();
            return;
        }

        float distanceFromTarget = Vector3.Distance(controller.transform.position, controller.lockonTarget.transform.position);
        if(distanceFromTarget > detectPlayerRange + detectPlayerRangeBuffer) {
            controller.lockonTarget = null;
            StopShooting();
            return;
        }

        StartShooting();
    }
}
