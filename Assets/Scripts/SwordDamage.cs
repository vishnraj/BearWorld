using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : DamageDealer {
    public bool attacking = false;
    public bool entered = false;
    public float damage;

    // Use this for initialization
    void Start () {
         
    }

    private void Awake() {
        damager_name = Weapon.DamagerNames.BLADE;
    }

    public override float DealDamage(float health) {
        health -= damage;
        entered = true;
        
        return health;
    }

    public override void Init() {
        base.Init();

        damager_name = Weapon.DamagerNames.BLADE;
    }
}
