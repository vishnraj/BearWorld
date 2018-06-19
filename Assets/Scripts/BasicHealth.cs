using UnityEngine;

public class BasicHealth : MonoBehaviour {
    public float max_health;
    public float health;

    protected bool can_damage = true; // this is something to provide a few invincibility frames
                                      // when the enemy is first spawned - set it in derived class
                                      // components where needed, but by default it will be true

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
      
    }

    // this is used as a possible
    // way to commnicate health updates
    // to other components
    public virtual void Notify() { }

    public void Heal(float incoming_health) {
        if (health + incoming_health >= max_health) {
            health = max_health;
        } else {
            health += incoming_health;
        }

        Notify();
    }

    public void Damage(float incoming_damage) {
        if (can_damage) {
            health -= incoming_damage;
            Notify();
        }
    }

    private void OnTriggerEnter(Collider other) {
        DamageDealer d = other.gameObject.GetComponent<DamageDealer>();

        if (d != null && d.enabled && d.GetOriginTag() != tag) {
            if (can_damage) {
                health = d.DealDamage(health);
                Notify();
            }
        }
    }
}
