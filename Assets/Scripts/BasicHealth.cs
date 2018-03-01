using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHealth : MonoBehaviour {
    public float max_health;
    public float health;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
      
    }

    public void Heal(float incoming_health) {
        if (health + incoming_health >= max_health) {
            health = max_health;
        } else {
            health += incoming_health;
        }
    }

    public void Damage(float incoming_damage) {
        health -= incoming_damage;
    }

    private void OnTriggerEnter(Collider other) {
        DamageDealer d = other.gameObject.GetComponent<DamageDealer>();

        if (d != null && d.enabled && d.GetOriginTag() != tag) {
            health = d.DealDamage(health);
        }
    }
}
