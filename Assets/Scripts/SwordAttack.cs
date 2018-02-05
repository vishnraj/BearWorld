using UnityEngine;

public class SwordAttack : BasicWeapon {
    GameObject character;

    SwordDamage blade, tip;    

    bool equipped = false;
    bool attacking = false;

    // Use this for initialization
    void Start() {
        // Most of this relies on character so it's done here

        if (transform.parent != null && transform.parent.transform.parent != null) {
            character = transform.parent.transform.parent.gameObject;
            equipped = true;
        }

        blade = transform.Find("Blade").GetComponent<SwordDamage>();
        tip = transform.Find("Tip").GetComponent<SwordDamage>();
    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.MELEE;
    }

    // Update is called once per frame
    void Update () {
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

    public override void Attack() {
        transform.Rotate(0, 0, 90);
        attacking = true;
        blade.attacking = true;
        tip.attacking = true;
    }

    public override void EndAttack() {
        transform.Rotate(0, 0, -90);
        attacking = false;
        blade.attacking = false;
        tip.attacking = false;
    }
}
