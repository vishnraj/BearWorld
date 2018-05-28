using UnityEngine;
using System.Collections.Generic;
using InputEvents;
using InventoryEvents;

namespace Items {
    public class Ammo {
        public float ammo_amount;
        public GameObject ammo_type;

        public Ammo(float _amount, GameObject _type) {
            ammo_amount = _amount;
            ammo_type = _type;
        }
    }

    public class DropFactory {
        // (kind of a) Factory function
        public void Drop(List<string> drops, Vector3 pos) {
            System.Random rnd = new System.Random();
            int i = rnd.Next(0, drops.Count);

            GameObject dropped = Object.Instantiate(Resources.Load("Prefabs/" + drops[i]), pos, new Quaternion()) as GameObject;
            dropped.GetComponent<BoxCollider>().enabled = true;
            dropped.GetComponent<Rigidbody>().isKinematic = false;
            dropped.GetComponent<Rigidbody>().mass = 1;
        }
    }
}

// Will determine how items are picked up and assigned 
// to player later as well as notifying other systems
// about these items 
public class Inventory : MonoBehaviour {
    public int max_items;
    public float offset_x;
    public string desired_equipped;
    public GameObject equipped = null;
    public GameObject event_manager;

    bool start = false;
    int current_inventory_index;
    int desired_equipped_index;
    List<string> inventory;
    Dictionary<string, Items.Ammo> ammo_inventory;

    InventoryPublisher publisher;
    PlayerAttackController pac;
    ThirdPersonTargetingSystem tps;
    BasicCharacter c;
    Weapon.WeaponFactory f;

    delegate void DoUpdate();
    DoUpdate update;

    // Use this for initialization
    private void Start() {
        desired_equipped = "";
        ammo_inventory = new Dictionary<string, Items.Ammo>();

        tps = GetComponent<ThirdPersonTargetingSystem>();
        pac = GetComponent<PlayerAttackController>();
        c = GetComponent<BasicCharacter>();
        f = new Weapon.WeaponFactory();

        desired_equipped_index = current_inventory_index = -1;

        c.SetAmmoAmount(0);
        c.SetAmmoType(null);


        publisher = event_manager.GetComponent<ComponentEventManager>().inventory_publisher;

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        update = DefaultUpdate;
    }

    private void Awake() {
        inventory = new List<string>();
    }

    // Update is called once per frame
    private void Update() {
        if (!start) {
            publisher.OnInventoryEvent(equipped, INVENTORY_EVENT.INIT);
            start = true;
        }

        update();
    }

    private void OnCollisionEnter(Collision collision) {
        GameObject item = collision.collider.gameObject;
        if (item != null && item.GetComponent<BasicWeapon>() != null) {
            pick_up(item);
        }
    }

    private void OnCollisionStay(Collision collision) {
        GameObject item = collision.collider.gameObject;
        if (item != null && item.GetComponent<BasicWeapon>() != null) {
            pick_up(item);
        }
    }

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    if (equipped != null) {
                        DeactivateItem();
                    }

                    update = PausedUpdate;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    if (equipped != null) {
                        ActivateItem();
                    }

                    if (desired_equipped == "") {
                        desired_equipped_index = current_inventory_index = -1;
                    }

                    update = DefaultUpdate;
                }
                
                break;
            default:
                break;
        }
    }

    void DefaultUpdate() {
        // When in action, we must keep the inventory up to date
        // but the updating off this could be done better
        // using pub/sub via observer - we can set this class
        // to observer the different items that are currently
        // in the player's possession
        if (c.GetAmmoType() != null && ammo_inventory[equipped.name].ammo_amount != c.GetAmmoAmount()) {
            ammo_inventory[equipped.name].ammo_amount = c.GetAmmoAmount(); // maintain consistency of inventory
            event_manager.GetComponent<ComponentEventManager>().inventory_publisher.OnInventoryEvent(c.GetAmmoAmount(), INVENTORY_EVENT.UPDATE_AMMO_AMOUNT);
        }
    }

    void PausedUpdate() {
        equip_item();
        unequip_item();
        update_equipped();
    }

    public void set_current_index() {
        if (desired_equipped == "") {
            current_inventory_index = 0;
        } else {
            current_inventory_index = desired_equipped_index;
        }
    }

    public void increment_current_index() {
        if (current_inventory_index + 1 == max_items) {
            current_inventory_index = 0;
        } else {
            ++current_inventory_index;
        }
    }

    public void decrement_current_index() {
        if (current_inventory_index - 1 < 0) {
            current_inventory_index = max_items - 1;
        } else {
            --current_inventory_index;
        }
    }

    public int get_current_index() {
        return current_inventory_index;
    }

    public List<string> get_inventory() {
        return inventory;
    }

    public void drop_item() {
        if (current_inventory_index < inventory.Count) {
            string dropped = inventory[current_inventory_index];

            if (desired_equipped_index == current_inventory_index) {
                desired_equipped = "";
                desired_equipped_index = -1;

                c.SetAmmoAmount(0);
                c.SetAmmoType(null);

                set_equipped_none();
            }

            inventory.RemoveAt(current_inventory_index);

            if (desired_equipped != "") {
                for (int i = 0; i < inventory.Count; ++i) {
                    if (inventory[i] == desired_equipped) {
                        desired_equipped_index = i;
                        break;
                    }
                }
            }

            GameObject item = (GameObject)Instantiate(Resources.Load("Prefabs/" + dropped));
            item.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            item.GetComponent<BoxCollider>().enabled = true;
            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Rigidbody>().mass = 1;

            if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                item.GetComponent<HeldAmmo>().ammo_amount = ammo_inventory[dropped].ammo_amount;
                ammo_inventory.Remove(dropped);
            }
        }
    }

    private void pick_up(GameObject item) {
        if (Input.GetButton("X")) {
            if (item.name.Contains(" ")) {
                item.name = item.name.Substring(0, item.name.LastIndexOf(" "));
            } else if (item.name.Contains("(")) {
                item.name = item.name.Substring(0, item.name.LastIndexOf("("));
            }

            if (inventory.Count < max_items && !inventory.Contains(item.name)) {
                inventory.Add(item.name);

                if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                    HeldAmmo ammo = item.GetComponent<HeldAmmo>();
                    ammo_inventory.Add(item.name, new Items.Ammo(ammo.ammo_amount, ammo.ammo_type));
                }

                Destroy(item);
            } else if (inventory.Contains(item.name)) {
                if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                    HeldAmmo ammo = item.GetComponent<HeldAmmo>();
                    if ((equipped == null) || (equipped != null && item.name != equipped.name)) {
                        increase_inventory_ammo(item.name, ammo.weapon_ammo_cap, ammo.ammo_amount, false);
                    } else {
                        increase_inventory_ammo(item.name, ammo.weapon_ammo_cap, ammo.ammo_amount, true);
                        c.SetAmmoAmount(ammo_inventory[item.name].ammo_amount);
                        publisher.OnInventoryEvent(c.GetAmmoAmount(), INVENTORY_EVENT.UPDATE_AMMO_AMOUNT);
                    }

                    Destroy(item);
                }
            }
        }
    }

    void increase_inventory_ammo(string item_name, float weapon_ammo_cap, float in_amount, bool is_equipped) {
        float current_amount = ammo_inventory[item_name].ammo_amount;
        if (!is_equipped && current_amount + in_amount >= weapon_ammo_cap) {
            ammo_inventory[item_name].ammo_amount = weapon_ammo_cap;
        } else if (is_equipped && c.GetAmmoAmount() + in_amount >= weapon_ammo_cap) {
            ammo_inventory[item_name].ammo_amount = weapon_ammo_cap;
        } else {
            if (!is_equipped) {
                ammo_inventory[item_name].ammo_amount += in_amount;
            } else {
                ammo_inventory[item_name].ammo_amount = c.GetAmmoAmount() + in_amount;
            }
        }
    }

    private void equip_item() {
        if (Input.GetButtonDown("A")) {
            if (inventory.Count != 0 && current_inventory_index < inventory.Count) {
                desired_equipped = inventory[current_inventory_index];
                desired_equipped_index = current_inventory_index;
            }

            set_ammo_type(desired_equipped);
        }
    }

    private void set_ammo_type(string weapon_name) {
        switch (weapon_name) {
            case "Raygun":
            case "Bombs": {
                c.SetAmmoAmount(ammo_inventory[weapon_name].ammo_amount);
                c.SetAmmoType(ammo_inventory[weapon_name].ammo_type);

                publisher.OnInventoryEvent(c.GetAmmoType().name, INVENTORY_EVENT.SET_AMMO);
                publisher.OnInventoryEvent(c.GetAmmoAmount(), INVENTORY_EVENT.UPDATE_AMMO_AMOUNT);
            }
            break;
            default: {
                    c.SetAmmoAmount(0);
                    c.SetAmmoType(null);

                    publisher.OnInventoryEvent(c.GetAmmoAmount(), INVENTORY_EVENT.UNSET_AMMO);
                }
                break;
        }
    }

    private void unequip_item() {
        if (Input.GetButtonDown("B")) {
            desired_equipped = "";
            c.SetAmmoAmount(0);
            c.SetAmmoType(null);
            set_equipped_none();
        }
    }

    public void update_equipped() {
        if ((desired_equipped != "" && equipped == null) || (desired_equipped != "" && equipped != null && desired_equipped != equipped.name)) {
            if (equipped != null) {
                Destroy(equipped);
            }

            equipped = f.SpawnEquipped(desired_equipped, transform, "RightArm");

            tps.current_weapon_range = equipped.GetComponent<BasicWeapon>().range;

            publisher.OnInventoryEvent(equipped, INVENTORY_EVENT.EQUIP);
        }
    }

    private void set_equipped_none() {
        if (equipped != null) {
            publisher.OnInventoryEvent(equipped, INVENTORY_EVENT.UNEQUIP);

            Destroy(equipped);
            equipped = null;
        }
    }

    void ActivateItem() {
        pac.weapon = equipped.GetComponent<BasicWeapon>();
        pac.weapon.SetCharacter(c);
        pac.weapon.enabled = true;
        pac.enabled = true;
    }

    void DeactivateItem() {
        pac.weapon.enabled = false;
        pac.enabled = false;
    }
}
