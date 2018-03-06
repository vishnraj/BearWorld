using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosion : MonoBehaviour {
    public float radius;
    public float power;
    public float upward;

    bool exploded = false; // replace with state pattern later
    EnemyHealth h;

	// Use this for initialization
	void Start () {
        h = GetComponent<EnemyHealth>();
        
        if (h == null) {
            Debug.LogError("We could not find an enemy health script attached to this object. Exiting.");
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    private void FixedUpdate() {
        if (!exploded && h.health <= 0) {
            Explode();
        }
    }

    void Explode() {
        // in order to maintain old position
        // we must disable forces on this rigidbody
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Collider>().enabled = false;

        for (int i = 0; i < transform.childCount; ++i) {
            Transform c = transform.GetChild(i);
            EnemyHealth c_health = c.GetComponent<EnemyHealth>();
            if (c_health == null) {
                continue;
            }

            Rigidbody rb = c.gameObject.GetComponent<Rigidbody>();
            if (rb == null) {
                continue;
            }

            Collider col = c.gameObject.GetComponent<Collider>();
            if (col == null) {
                continue;
            }

            rb.isKinematic = false;
            //rb.useGravity = true;
            col.enabled = true;
            c_health.enabled = true;

            rb.AddExplosionForce(power, transform.position, radius, upward);
        }

        exploded = true;
    }
}
