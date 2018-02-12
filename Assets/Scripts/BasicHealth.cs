using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHealth : MonoBehaviour {
    public float health;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (health <= 0) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        DamageDealer d = other.gameObject.GetComponent<DamageDealer>();

        if (d != null && d.GetOriginTag() != tag) {
            health = d.DealDamage(health);
        }
    }
}
