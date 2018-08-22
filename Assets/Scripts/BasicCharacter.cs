using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacter : MonoBehaviour {
    // This should represent the character's
    // overall state (where other componenents
    // are responsible for tasks that are specific
    // to them, this keeps track of overall
    // important state information for the character)

    GameObject current_ammo_type = null;
    float current_ammo_amount = 0;

    bool in_lock_on = false;
    GameObject target;
    Vector3 aiming_direction;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetAmmoType(GameObject ammo_type) { current_ammo_type = ammo_type; }
    public GameObject GetAmmoType() { return current_ammo_type; }

    public void SetAmmoAmount(float amount) { current_ammo_amount = amount; }
    public float GetAmmoAmount() { return current_ammo_amount; }
    public void DecrementAmmoAmount() { --current_ammo_amount; }

    // some of the below functions should actually get moved out to a derived
    // player class - it's unlikely that enemies will have use for it
    // and enemies are using this class as well

    public void SetTarget(GameObject _target) { target = _target; }
    public GameObject GetTarget() { return target;  }

    public void SetAimingDirection(Vector3 direction) { aiming_direction = direction; }
    public Vector3 GetAimingDirection() { return aiming_direction; }

    public void SetInLockOn(bool locked_on) { in_lock_on = locked_on; }
    public bool InLockOn() { return in_lock_on; }
}
