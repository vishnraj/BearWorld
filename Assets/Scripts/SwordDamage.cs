using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SwordDamage : DamageDealer {
    public bool attacking = false;
    public bool entered = false;
    public float damage;

    // Use this for initialization
    void Start () {
         
    }

    private void Awake() {

    }

    public override float DealDamage(float health) {
        health -= damage;
        entered = true;
        
        return health;
    }
}
