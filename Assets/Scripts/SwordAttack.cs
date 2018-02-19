using UnityEngine;

public class SwordAttack : BasicWeapon {
    SwordDamage blade;

    int current_sequence = 0;

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
        transform.Rotate(0, 0, 90);
    }

    public override void EndAttack() {
        transform.Rotate(0, 0, -90);
    }

    public override void Init() {
        base.Init();
        transform.Rotate(0, 75, 0);
        transform.Rotate(-90, 0, 0);
    }
}
