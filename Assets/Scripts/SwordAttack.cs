using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour {
    GameObject player;
    ItemSystem player_item_system;
    ThirdPersonTargetingSystem tps;

    SwordDamage blade, tip;    

    bool equipped = false;
    bool attacking = false;

    // Use this for initialization
    void Start() {
        if (transform.parent != null && transform.parent.transform.parent != null) {
            player = transform.parent.transform.parent.gameObject;
            player_item_system = player.GetComponent<ItemSystem>();
            tps = player.GetComponent<ThirdPersonTargetingSystem>();
            equipped = true;
        }

        blade = transform.Find("Blade").GetComponent<SwordDamage>();
        tip = transform.Find("Tip").GetComponent<SwordDamage>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("RightTriggerAxis") > 0 && !attacking) {
            transform.Rotate(0, 0, -90);
            attacking = true;
            blade.attacking = true;
            tip.attacking = true;
        } else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
            transform.Rotate(0, 0, 90);
            attacking = false;
            blade.attacking = false;
            tip.attacking = false;
        }

        if (blade.attacking && blade.entered) {
            blade.attacking = false;
        }

        if (tip.attacking && tip.entered) {
            tip.attacking = false;
        }

        if (attacking && !tip.entered && !blade.entered) {
            blade.attacking = tip.attacking = true;
        }
    }
}
