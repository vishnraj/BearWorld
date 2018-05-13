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
        health -= incoming_damage;
        Notify();
    }

    private void OnTriggerEnter(Collider other) {
        DamageDealer d = other.gameObject.GetComponent<DamageDealer>();

        if (d != null && d.enabled && d.GetOriginTag() != tag) {
            health = d.DealDamage(health);
            Notify();
        }
    }
}
