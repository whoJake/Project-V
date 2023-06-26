using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController, IDamageable {
    [SerializeField] float maxHealth;
    [SerializeField] float health;

    float IDamageable.MaxHealth { get { return maxHealth; } 
                                  set { maxHealth = value; } }
    float IDamageable.Health { get { return health; } 
                               set { health = value; } }

    void IDamageable.OnDamage(float amount) {
        StartCoroutine(DamageEffect());
    }

    private IEnumerator DamageEffect() {
        Material m = GetComponent<MeshRenderer>().material;
        m.color = Color.red;
        yield return new WaitForSeconds(0.01f);
        m.color = Color.white;
    }

}
