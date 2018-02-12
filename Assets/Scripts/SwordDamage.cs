using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SwordDamage : DamageDealer {
    public bool attacking = false;
    public bool entered = false;

    // Use this for initialization
    void Start () {
         
    }

    private void Awake() {

    }

    public override float DealDamage(float health) {
        health -= 3;
        entered = true;
        
        return health;
    }
}
