using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stat Bundle")]
public class StatBundle : ScriptableObject
{
    [Min(0)] public float flatMovementSpeed;
    [Min(0)] public float percentMovementSpeed;

    [Min(0)] public float flatHealthPoints;
    [Min(0)] public float percentHealthPoints;

    [Min(0)] public float flatJumpHeight;
    [Min(0)] public float percentJumpHeight;

    public void ApplyToStatHandler(StatHandler target) {
        target.bonusMovementSpeed += flatMovementSpeed;
        target.bonusMovementSpeed += target.movementSpeed * percentMovementSpeed;

        target.bonusHealthPoints += flatHealthPoints;
        target.bonusHealthPoints += target.healthPoints * percentHealthPoints;

        target.bonusJumpHeight += flatJumpHeight;
        target.bonusJumpHeight += target.jumpHeight * percentJumpHeight;
    }
}
