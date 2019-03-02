using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : DamageDealer {
    public bool attacking = false;

    HashSet<int> damage_dealt_to_enemy; // for this swing, did we already hit the given enemy?
    public float damage;

    // Use this for initialization
    void Start () {
                 
    }

    private void OnEnable() {
        damage_dealt_to_enemy.Clear();
    }

    private void Awake() {
        damager_name = Weapon.DamagerNames.BLADE;
        damage_dealt_to_enemy = new HashSet<int>();
    }

    public override float DealDamage(float health, int id) {
        if (damage_dealt_to_enemy.Contains(id)) {
            return health;
        }
        health -= damage;
        damage_dealt_to_enemy.Add(id);
        
        return health;
    }

    public override void Init() {
        base.Init();

        damager_name = Weapon.DamagerNames.BLADE;
    }
}
