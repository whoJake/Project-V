using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Movement Providers/Kite Zones")]
public class KiteZoneMovement : MovementProvider {

    [SerializeField] private Zone[] zones;

    private Zone GetZone() {
        float dstFromTarget = Vector3.Distance(target.position, controller.transform.position);
        for(int i = 0; i < zones.Length; i++) {
            if (dstFromTarget < zones[i].radius)
                return zones[i];
        }
        return null;
    }

    public override MovementState GetMovementState() {
        Vector2 direction = Vector2.zero;
        float moveSpeed = 0f;
        Zone currentZone = GetZone();

        if(currentZone != null && !(canOverride && !canMove)) {
            if (target) {
                Vector3 vec2target = target.position - controller.transform.position;
                direction = new Vector2(vec2target.x, vec2target.z).normalized;
            }

            canOverride = currentZone.overridable;
            moveSpeed = currentZone.moveSpeed;

            switch (currentZone.kiteDir) {
                case Zone.KiteDirection.Forward:
                    controller.BehaviourProvider?.Enable();
                    break;
                case Zone.KiteDirection.Backward:
                    controller.BehaviourProvider?.Enable();
                    direction *= -1;
                    break;
                case Zone.KiteDirection.None:
                    controller.BehaviourProvider?.Disable();
                    direction *= 0;
                    break;
            }
        } else {
            controller.BehaviourProvider?.Disable();
        }

        return new MovementState() {
            direction = direction,
            speed = moveSpeed,
            isCrouched = false
        };
    }

    [System.Serializable]
    public class Zone {
        [Min(0)] public float radius;
        public KiteDirection kiteDir;
        public float moveSpeed;
        public bool overridable;

        [System.Serializable]
        public enum KiteDirection {
            Forward,
            Backward,
            None
        }
    }
}
