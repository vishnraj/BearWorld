using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputEvents;

public class PlayerAttackController : MonoBehaviour {
    public BasicWeapon weapon = null;

    BasicCharacter character;
    bool attacking = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (weapon != null) {
            if (Input.GetAxis("RightTriggerAxis") > 0 && !attacking) {
                weapon.Attack();
                attacking = true;
            } else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
                weapon.EndAttack();
                attacking = false;
            }
        }
    }
}
