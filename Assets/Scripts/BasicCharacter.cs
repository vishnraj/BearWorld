using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacter : MonoBehaviour {
    // This should represent the character's
    // overall state (where other componenents
    // are responsible for tasks that are specific
    // to them, this keeps track of overall
    // important state information for the character)

    protected GameObject current_ammo_type = null;
    protected float current_ammo_amount = 0;

    protected GameObject target;
    protected Vector3 aiming_direction;

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

    public void SetTarget(GameObject _target) { target = _target; }
    public GameObject GetTarget() { return target; }

    public void SetAimingDirection(Vector3 direction) { aiming_direction = direction; }
    public Vector3 GetAimingDirection() { return aiming_direction; }
}
