using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDamage : MonoBehaviour {
    // As far as parent class for this, when
    // we start having more bombs, this will be
    // typed a Damage Emitter vs Damage Dealer

    public float lifetime;
    public float range;
    public float damage;

    float start;
    bool expired = false;
    string origin_tag;

	// Use this for initialization
	void Start () {
        start = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (!expired && Time.time - start >= lifetime) {
            expired = true;
            Explode();
        }		
	}

    void DealDamage() {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, range);
        foreach (Collider col in objectsInRange) {
            BoxCollider b_col = col as BoxCollider; // as of now, character game objects
                                                    // are going to use a box collider
            if (b_col != null && b_col.tag != origin_tag) {
                BasicHealth h = b_col.gameObject.GetComponent<BasicHealth>();
                if (h != null) {
                    // linear falloff of effect
                    float proximity = (transform.position - col.transform.position).magnitude;
                    float effect = 1 - (proximity / range); // outer most won't receive damage
                                                              // cannot be negative since this
                                                              // only applies to colliders in range

                    int actual_damage = (int) (damage * effect);
                    h.Damage(actual_damage);
                }   
            }
        }
    }

    void Explode() {
        ParticleSystem exp = GetComponent<ParticleSystem>();
        exp.Play();
        DealDamage();
        Destroy(gameObject, exp.main.duration);
    }

    public void SetOriginTag(string t) {
        origin_tag = t;
    }

    public string GetOriginTag() {
        return origin_tag;
    }
}
