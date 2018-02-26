using System.Collections;
using UnityEngine;
using Utility;

public class BombAttack : BasicWeapon {
    bool dropped = false;

    Searching s;

    // Use this for initialization
    void Start() {

    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.RANGE;
        s = new Searching();
    }

    // Update is called once per frame
    void Update() {

    }

   void Drop() {
        if (c.GetAmmoAmount() != 0) {
            GameObject g = s.FindComponentUpHierarchy<BasicHealth>(transform);
            if (g != null) {
                BoxCollider col = g.GetComponent<BoxCollider>();

                if (col == null) {
                    Debug.LogError("The game object that holds an item must have a box collider. Cannot proceed.");
                    return;
                }

                Vector3 in_front = g.transform.position + (g.transform.forward) * col.size.z;
                GameObject bomb = Instantiate(c.GetAmmoType(), in_front, transform.rotation) as GameObject;
                BombDamage b = bomb.GetComponent<BombDamage>();
                b.SetOriginTag(c.tag);

                c.DecrementAmmoAmount();

                b.enabled = true;
                dropped = true;
            } else {
                Debug.LogError("Could not find the health script of highest level parent that this item is equipped to. Cannot proceed.");
            }
        }
    }

    public override void Attack() {
        if (!dropped) {
            Drop();
        }
    }

    public override void EndAttack() {
        dropped = false;
    }
}
