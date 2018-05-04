using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public GameObject HUD;
    public GameObject none;
    public Sprite selection;
    public GameObject event_manager;

    Transform crosshair;
    bool switching = false;
    int current_inventory_index;
    int desired_equipped_index;
    float start_location_x;
    float start_location_y;
    float start_location_z;
    List<string> inventory;
    Dictionary<string, Items.Ammo> ammo_inventory;
    List<GameObject> menu_objects;
    Transform equipped_icon;
    Transform equipped_status;
    Transform ammo_icon;
    Transform ammo_remaining;
    GameObject highlighter;
    GameObject current_target = null;

    PlayerAttackController pac;
    ThirdPersonTargetingSystem tps;
    BasicCharacter c;
    Weapon.WeaponFactory f;

    public enum INVENTORY_STATE { DEFAULT, PAUSED};

    INVENTORY_STATE s;

    // Use this for initialization
    private void Start() {
        desired_equipped = "";
        ammo_inventory = new Dictionary<string, Items.Ammo>();
        menu_objects = new List<GameObject>();

        tps = GetComponent<ThirdPersonTargetingSystem>();
        pac = GetComponent<PlayerAttackController>();
        c = GetComponent<BasicCharacter>();
        f = new Weapon.WeaponFactory();

        equipped_icon = HUD.transform.Find("EquippedIcon");
        equipped_status = HUD.transform.Find("EquippedStatus");

        ammo_icon = HUD.transform.Find("AmmoIcon");
        ammo_remaining = HUD.transform.Find("AmmoRemaining");

        crosshair = HUD.transform.Find("Crosshair");

        max_items = 3;
        desired_equipped_index = current_inventory_index = -1;

        set_equipped_none();
        set_ammo_none();

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
        s = INVENTORY_STATE.DEFAULT;
    }

    private void Awake() {
        inventory = new List<string>();
    }

    // Update is called once per frame
    private void Update() {
        switch(s) {
            case INVENTORY_STATE.PAUSED:
                DoPausedUpdate();
                break;
            default:
                DoDefaultUpdate();
                break;
        }
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

    void GlobalInputEventsCallback(object sender, InputEvents.INPUT_EVENT e) {
        switch (e) {
            case InputEvents.INPUT_EVENT.PAUSE: {
                    if (equipped != null) {
                        DeactivateItem();
                    }
                    s = INVENTORY_STATE.PAUSED;
                }
                break;
            case InputEvents.INPUT_EVENT.UNPAUSE: {
                    // The only reason we do this here
                    // is because order matters (this
                    // engine sets order for components 
                    // at runtime and this order changes
                    // between runs, so we need this to
                    // make sure controller sees weapon
                    if (equipped != null) {
                        ActivateItem();
                    }

                    // we do this only when we are about to turn off the menu
                    // as at this point the current_inventory_index will not
                    // be used, so it will be safe to do so
                    if (desired_equipped == "") {
                        desired_equipped_index = current_inventory_index = -1; // for consistency
                                                                               // we don't do anything
                                                                               // with -1 case yet
                    }

                    s = INVENTORY_STATE.DEFAULT;
                }
                
                break;
            default:
                break;
        }
    }

    void DoDefaultUpdate() {
        // When in action, we must keep the inventory up to date
        // but the updating off this could be done better
        // using pub/sub via observer - we can set this class
        // to observer the different items that are currently
        // in the player's possession
        if (c.GetAmmoType() != null && ammo_inventory[equipped.name].ammo_amount != c.GetAmmoAmount()) {
            ammo_inventory[equipped.name].ammo_amount = c.GetAmmoAmount(); // maintain consistency of inventory
            update_ammo_remaining();
        }
    }

    void DoPausedUpdate() {
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
                desired_equipped_index = -1; // consistency

                // Unequip the item effectively
                // If it is the one that we are dropping
                set_ammo_none();
                set_equipped_none();
            }

            inventory.RemoveAt(current_inventory_index);

            // need to refind the desired_equipped item
            // if it has moved
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
                        c.SetAmmoAmount(ammo_inventory[item.name].ammo_amount); // this needs to get updated for other objects to see
                        update_ammo_remaining(); // Update GUI
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

                update_ammo_type();
                update_ammo_remaining(); // Update on GUI
            }
            break;
            default:
                set_ammo_none();
                break;
        }
    }

    private void unequip_item() {
        if (Input.GetButtonDown("B")) {
            desired_equipped = "";
            set_ammo_none();
            set_equipped_none();
        }
    }

    private void update_ammo_remaining() {
        ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo Amount: " + c.GetAmmoAmount().ToString();

        if (c.GetAmmoAmount() > 5) {
            ammo_remaining.gameObject.GetComponent<Text>().color = new Color(148, 0, 211);
        } else {
            ammo_remaining.gameObject.GetComponent<Text>().color = new Color(148, 0, 211);
        }
    }

    private void update_ammo_type() {
        Sprite s = Resources.Load<Sprite>("Sprites/" + c.GetAmmoType().name);
        if (s != null) {
            ammo_icon.gameObject.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
        }
    }

    public void update_equipped() {
        // Set up the new equipped
        // Update GUI elements (later through event system)
        if ((desired_equipped != "" && equipped == null) || (desired_equipped != "" && equipped != null && desired_equipped != equipped.name)) {
            if (equipped != null) {
                Destroy(equipped);
            }

            equipped = f.SpawnEquipped(desired_equipped, transform, "RightArm");

            tps.current_weapon_range = equipped.GetComponent<BasicWeapon>().range;

            equipped_status.gameObject.GetComponent<Text>().text = equipped.name + " equipped";
            equipped_status.gameObject.GetComponent<Text>().color = new Color(148, 0, 211);

            Sprite s = Resources.Load<Sprite>("Sprites/" + equipped.name);
            if (s != null) {
                equipped_icon.gameObject.GetComponent<Image>().sprite = s;
            } else {
                Debug.LogError("We had an issue generating the asset preview");
            }

            crosshair.GetComponent<AimingSystem>().enabled = true;
        }
    }

    private void set_equipped_none() {
        equipped_status.gameObject.GetComponent<Text>().text = "None equipped";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(255, 255, 0);

        Sprite s = Resources.Load<Sprite>("Sprites/" + none.name);
        if (s != null) {
            equipped_icon.gameObject.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
        }

        crosshair.GetComponent<AimingSystem>().enabled = false;

        Destroy(equipped);
    }

    private void set_ammo_none() {
        c.SetAmmoAmount(0);
        c.SetAmmoType(null);

        ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo not set ";
        ammo_remaining.gameObject.GetComponent<Text>().color = new Color(255, 255, 0);

        Sprite s = Resources.Load<Sprite>("Sprites/" + none.name);
        if (s != null) {
            ammo_icon.gameObject.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
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
