using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SwordDamage : MonoBehaviour {
    public bool attacking = false;
    public bool entered = false;

    Searching s;

    // Use this for initialization
    void Start () {
         s = new Searching();
    }
	
    void OnTriggerEnter(Collider collide) {
        if (collide.tag == "Enemy" && attacking && !entered) {
            GameObject obj = s.FindComponentUpHierarchy<EnemyHealth>(collide.transform);
            if (obj != null) {
                // eventually there can be a function in EnemyHealth that takes input of bodypart
                // hashed to the normal amount that is lost for said body part
                // until that body part is destroyed
                obj.GetComponent<EnemyHealth>().health -= 1;
                entered = true;
            }
        }
    }

    void OnTriggerExit(Collider collide) {
        if (collide.tag == "Enemy" && entered) {
            entered = false;
        }
    }
}
