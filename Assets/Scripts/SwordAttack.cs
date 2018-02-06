using UnityEngine;

public class SwordAttack : BasicWeapon {
    GameObject character;

    SwordDamage blade;

    bool equipped = false;
    bool attacking = false;

    int current_sequence = 0;

    // Use this for initialization
    void Start() {
        // Most of this relies on character so it's done here

        if (transform.parent != null && transform.parent.transform.parent != null) {
            character = transform.parent.transform.parent.gameObject;
            equipped = true;
        }

        blade = transform.Find("Blade").GetComponent<SwordDamage>();
    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.MELEE;
    }

    // Update is called once per frame
    void Update () {
        if (blade.attacking && blade.entered) {
            blade.attacking = false;
        }

        if (attacking && !blade.entered) {
            blade.attacking = true;
        }
    }

    public override void Attack() {
        if (current_sequence == 0) {
            transform.Rotate(0, 0, 90);
        } else if (current_sequence == 1) {
            transform.Rotate(0, 0, 90);
        }

        attacking = true;
        blade.attacking = true;
    }

    public override void EndAttack() {
        if (current_sequence == 0) {
            transform.Rotate(0, 0, -90);
            transform.Rotate(-90, 0, 0);
            current_sequence = 1;
        } else if (current_sequence == 1) {
            transform.Rotate(0, 0, -90);
            transform.Rotate(90, 0, 0);
            current_sequence = 0;
        }

        attacking = false;
        blade.attacking = false;
    }
}
