using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public bool active = true;

    public abstract void OnInteract(GameObject target);
}
