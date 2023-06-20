using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourProvider : ScriptableObject
{
    protected GameObject target;

    public abstract void Initialize(GameObject target);
    public abstract void OnFrameUpdate();
    public abstract void OnPhysicsUpdate();
}
