using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    [Tooltip("Base health")]
    [Min(0)]          public float baseHealthPoints;
    [HideInInspector] public float bonusHealthPoints;
                      public float healthPoints { get { return baseHealthPoints + bonusHealthPoints; } }

    [Tooltip("Base movement speed in units per second")]
    [Min(0)]          public float baseMovementSpeed;
    [HideInInspector] public float bonusMovementSpeed;
                      public float movementSpeed { get { return baseMovementSpeed + bonusMovementSpeed; } }

    [Tooltip("Base sprint multiplier")]
    [Min(1)]          public float baseSprintMultiplier;
    [HideInInspector] public float bonusSprintMultiplier;
                      public float sprintMultiplier { get { return baseSprintMultiplier + bonusSprintMultiplier; } }

    [Tooltip("Jump Height in units")]
    [Min(0)]          public float baseJumpHeight;
    [HideInInspector] public float bonusJumpHeight;
                      public float jumpHeight { get { return baseJumpHeight + bonusJumpHeight; } }


    //Interact with items
    private void OnTriggerEnter(Collider other) {
        PickupItem pickupItem;
        if(other.gameObject.TryGetComponent<PickupItem>(out pickupItem)) {
            pickupItem.OnInteract(gameObject);
        }
    }
}
