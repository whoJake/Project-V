using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Interactable
{
    public StatBundle itemStats;
    //Item model
    //Item tier
    //Item element

    public override void OnInteract(GameObject target) {
        if (!active) return;

        StatHandler targetHandler;
        if (target.TryGetComponent<StatHandler>(out targetHandler)) {
            active = false;

            targetHandler.bonusMovementSpeed += itemStats.flatMovementSpeed;
            targetHandler.bonusMovementSpeed += targetHandler.movementSpeed * itemStats.percentMovementSpeed;

            targetHandler.bonusHealthPoints += itemStats.flatHealthPoints;
            targetHandler.bonusHealthPoints += targetHandler.healthPoints * itemStats.percentHealthPoints;

            //Play some pickup animation
        }
    }
}
