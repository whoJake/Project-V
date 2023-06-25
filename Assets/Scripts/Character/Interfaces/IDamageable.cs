using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public float MaxHealth { get; protected set; }
    public float Health { get; protected set; }
    public void Damage(float amount) {
        Health -= amount;
        OnDamage(amount);
        if(Health <= 0f) {
            Health = 0;
            Kill();
        }
    }
    protected void OnDamage(float amount) { }

    public void Heal(float amount) {
        Health = Mathf.Min(MaxHealth, Health + amount);
        OnHeal(amount);
    }
    protected void OnHeal(float amount) { }

    public void Kill() { }
}
