using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon {
    public abstract class WeaponInfo {}
    public enum WEAPON_TYPE { RANGE, MELEE }
}

public abstract class BasicWeapon : MonoBehaviour {
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

    public Weapon.WEAPON_TYPE GetWeaponType() { return type; }
    public void SetCharacter(BasicCharacter character) { c = character; }
}
