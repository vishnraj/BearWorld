using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDamage : DamageDealer {
    public float lifetime;
    public float range;
    public float damage;

    float start;
    bool expired = false;

	// Use this for initialization
	void Start () {
        start = Time.time;
	}

    private void Awake() {
        damager_name = Weapon.DamagerNames.BOMB;
    }

    // Update is called once per frame
    void Update () {
        if (!expired && Time.time - start >= lifetime) {
            expired = true;
            Explode();
        }		
	}

    void DamageEmission() {
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
        DamageEmission();
        Destroy(gameObject, exp.main.duration);
    }

    public override void Init() {
        base.Init();

        damager_name = Weapon.DamagerNames.BOMB;
    }
}
