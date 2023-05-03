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
}
