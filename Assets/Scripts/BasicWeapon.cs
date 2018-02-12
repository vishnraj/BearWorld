using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon {
    public abstract class WeaponInfo { }
    public enum WEAPON_TYPE { RANGE, MELEE }

    class WeaponFactory {
        // (kind of a) Factory function
        public GameObject SpawnEquipped(string desired_equipped, Transform _parent) {
            GameObject equipped = Object.Instantiate(Resources.Load("Prefabs/" + desired_equipped), _parent.position, _parent.rotation) as GameObject;
            equipped.transform.parent = _parent;
            if (equipped.name.Contains(" ")) {
                equipped.name = equipped.name.Substring(0, equipped.name.LastIndexOf(" "));
            } else if (equipped.name.Contains("(")) {
                equipped.name = equipped.name.Substring(0, equipped.name.LastIndexOf("("));
            }
            
            
            Object.Destroy(equipped.GetComponent<Rigidbody>());
            Object.Destroy(equipped.GetComponent<BoxCollider>());

            return equipped;
        }
    }
}

public abstract class BasicWeapon : MonoBehaviour {
    public float range;

    protected Weapon.WEAPON_TYPE type;
    protected BasicCharacter c;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void Attack() { }
    public virtual void EndAttack() { }

    protected virtual void Init() {
        if (transform.parent == null) {
            Debug.Log("Error, weapon not assigned to parent.");
            return;
        }

        Vector3 pos = transform.position;
        pos += transform.forward * .3f;
        pos += transform.up * .2f;
        transform.position = pos;
    }

    public Weapon.WEAPON_TYPE GetWeaponType() { return type; }
    public void SetCharacter(BasicCharacter character) { c = character; }
}
