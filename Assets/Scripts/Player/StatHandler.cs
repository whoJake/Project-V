using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    [Tooltip("Base movement speed of entity")]
    [Min(0)]          public float baseMovementSpeed;
    [HideInInspector] public float bonusMovementSpeed;
                      public float movementSpeed { get { return baseMovementSpeed + bonusMovementSpeed; } }

    [Tooltip("Base health of entity")]
    [Min(0)]          public float baseHealthPoints;
    [HideInInspector] public float bonusHealthPoints;
                      public float healthPoints { get { return baseHealthPoints + bonusHealthPoints; } }
}
