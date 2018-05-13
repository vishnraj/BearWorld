using UnityEngine;

public class SwordAttack : BasicWeapon {
    SwordDamage blade;

    // Use this for initialization
    void Start() {
        blade = transform.Find("Blade").GetComponent<SwordDamage>();
        blade.SetOriginTag(c.tag);
    }

    private void Awake() {
        type = Weapon.WEAPON_TYPE.MELEE;
    }

    // Update is called once per frame
    void Update () {
       
    }

    public override void Attack() {
        blade.enabled = true;
        transform.Rotate(0, 0, 90);
    }

    public override void EndAttack() {
        blade.enabled = false;
        transform.Rotate(0, 0, -90);
    }

    public override void Init() {
        base.Init();

        Vector3 pos = transform.position;
        pos += transform.forward * .3f;
        pos += transform.up * .2f;
        transform.position = pos;

        transform.Rotate(0, 75, 0);
        transform.Rotate(-90, 0, 0);
    }
}
