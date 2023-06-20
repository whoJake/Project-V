using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Movement Providers/User Movement")]
public class UserMovement : MovementProvider {
    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float jumpPower;

    public override void Initialize(GameObject target) {
        this.target = target;
    }

    public override MovementState GetMovementState() {
        if (Input.GetButtonDown("Jump")) {
            OnJump?.Invoke(jumpPower);
        }

        return new MovementState {  direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized,
                                    speed = movementSpeed,
                                    isCrouched = false
                                    };
    }
}
