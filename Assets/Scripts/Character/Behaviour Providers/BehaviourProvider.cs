using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourProvider : ScriptableObject
{
    protected EntityController controller;

    public abstract void Initialize(EntityController _controller);
    public abstract void OnFrameUpdate();
    public abstract void OnPhysicsUpdate();
}
