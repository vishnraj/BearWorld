﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Items {
    public class Ammo {
        public int ammo_amount;
        public GameObject ammo_type;

        public Ammo(int _amount, GameObject _type) {
            ammo_amount = _amount;
            ammo_type = _type;
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

    public GameObject equipped;

    public GameObject HUD;
    public GameObject none;
    public Sprite selection;

    Transform crosshair;
    bool item_menu_on;
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

    PlayerAttackController pac;
    ThirdPersonTargetingSystem tps;
    BasicCharacter c;

    // Use this for initialization
    private void Start() {
        desired_equipped = "";
        inventory = new List<string>();
        ammo_inventory = new Dictionary<string, Items.Ammo>();
        menu_objects = new List<GameObject>();

        tps = GetComponent<ThirdPersonTargetingSystem>();
        pac = GetComponent<PlayerAttackController>();
        c = GetComponent<BasicCharacter>();

        equipped_icon = HUD.transform.Find("EquippedIcon");
        equipped_status = HUD.transform.Find("EquippedStatus");

        ammo_icon = HUD.transform.Find("AmmoIcon");
        ammo_remaining = HUD.transform.Find("AmmoRemaining");

        crosshair = HUD.transform.Find("Crosshair");

        max_items = 3;
        desired_equipped_index = current_inventory_index = -1;

        set_equipped_none();
        set_ammo_none();
    }

    // Update is called once per frame
    private void Update() {
        // Most of these functions are checking for input
        // They should therefore not be called here, only things
        // that need to update the GUI, but even that can be handled
        // via message communication -> input handler will send a message
        // to this script, where the message can then be checked and the appropriate
        // action leads to the function getting called - this also means we don't unecessarily
        // call functions here, but can instead block in this script until we get the appropriate
        // type of message to begin taking a particular course of action

        toggle_item_menu();
        cycle_through_items();
        equip_item();
        unequip_item();
        drop_item();

        // Status of the equipped
        if ((equipped == null && desired_equipped != "") || (equipped != null && desired_equipped != equipped.name)) { 
            update_equipped();
        }

        // Status of other parts of player
        if (item_menu_on == true) {
            if (GetComponent<XboxOneControllerThirdPersonMovement>().enabled) {
                GetComponent<XboxOneControllerThirdPersonMovement>().enabled = false;
            }

            if (equipped != null && equipped.GetComponent<BasicWeapon>().enabled) {
                DeactivateItem();
            }
        } else {
            if (!GetComponent<XboxOneControllerThirdPersonMovement>().enabled) {
                GetComponent<XboxOneControllerThirdPersonMovement>().enabled = true;
            }

            if (equipped != null && !equipped.GetComponent<BasicWeapon>().enabled) {
                ActivateItem();
            } else if (equipped == null && pac.enabled) {
                DeactivateItem();
            }

            // When in action, we must keep the inventory up to date
            if (c.GetAmmoType() != null && ammo_inventory[equipped.name].ammo_amount != c.GetAmmoAmount()) {
                ammo_inventory[equipped.name].ammo_amount = c.GetAmmoAmount(); // maintain consistency of inventory
                update_ammo_remaining();
            }
        }
    }

    // Disabling this for now, we get issues when calling
    // both on OnTriggerStay and OnTriggerEnter - race
    // condiition, although strange because game loop
    // should be single threaded, with this getting
    // called first and stay after, but this call seems
    // to invalidate the stay call, breaking the game
    //private void OnTriggerEnter(Collider other) {
    //    GameObject item = other.gameObject;
    //    if (item != null && item.GetComponent<BasicWeapon>() != null) {
    //        pick_up(item);
    //    }
    //}


    private void OnTriggerStay(Collider other) {
        GameObject item = other.gameObject;
        if (item != null && item.GetComponent<BasicWeapon>() != null) {
            pick_up(item);
        }
    }

    private void pick_up(GameObject item) {
        // can only happen during
        // active game state
        if (item_menu_on) {
            return;
        }

        if (Input.GetButtonDown("X")) {
            if (inventory.Count < max_items && !inventory.Contains(item.name)) {
                if (item.name.Contains("("))
                    item.name = item.name.Substring(0, item.name.LastIndexOf("("));

                inventory.Add(item.name);

                if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                    HeldAmmo ammo = item.GetComponent<HeldAmmo>();
                    ammo_inventory.Add(item.name, new Items.Ammo(ammo.ammo_amount, ammo.ammo_type));
                }
            } else if (inventory.Contains(item.name)) {
                if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                    HeldAmmo ammo = item.GetComponent<HeldAmmo>();
                    if (item.name != equipped.name) {
                        ammo_inventory[item.name].ammo_amount += ammo.ammo_amount;
                    } else {
                        ammo_inventory[item.name].ammo_amount = c.GetAmmoAmount() + ammo.ammo_amount;
                        c.SetAmmoAmount(ammo_inventory[item.name].ammo_amount); // this needs to get updated for other objects to see
                        update_ammo_remaining(); // Update GUI
                    }
                }
            }
            Destroy(item);
        }
    }

    private void select_item_on_menu() {
        GameObject selected_ui_component = menu_objects[current_inventory_index]; // has to be in here
                                                                                  // if not, very odd
        RectTransform rt = selected_ui_component.GetComponent<RectTransform>();
        RectTransform rt_highlight;

        if (highlighter == null) {
            highlighter = new GameObject();
            highlighter.transform.SetParent(HUD.transform, false);
            highlighter.AddComponent<RectTransform>();
            rt_highlight = highlighter.GetComponent<RectTransform>();
            rt_highlight.anchorMax = new Vector2(.5f, .5f);
            rt_highlight.anchorMin = new Vector2(.5f, .5f);
            rt_highlight.sizeDelta = new Vector2(rt_highlight.sizeDelta.x * .6f, rt_highlight.sizeDelta.y * .6f);
            highlighter.AddComponent<Image>();
            highlighter.GetComponent<Image>().sprite = selection;
            highlighter.transform.localScale = Vector3.one;
        }

        rt_highlight = highlighter.GetComponent<RectTransform>();
        rt_highlight.position = new Vector3(rt.position.x, rt.position.y, start_location_z);
        selected_ui_component.transform.SetParent(highlighter.transform);
    }

    private void deselect_item_on_menu() {
        menu_objects[current_inventory_index].transform.SetParent(HUD.transform);
    }

    private void set_item_menu_hud(string name, int current_index) {
        // create the object 
        // add rect transform and set the position
        // add image and set the sprite
        // set transform parent to UI
        // add it to menu_objects for later destruction

        GameObject ui_component = new GameObject();
        ui_component.transform.SetParent(HUD.transform, false);
        ui_component.AddComponent<RectTransform>();
        RectTransform rt = ui_component.GetComponent<RectTransform>();
        rt.position = new Vector3(start_location_x + (current_index + 1) * offset_x, start_location_y, start_location_z);
        rt.anchorMax = new Vector2(.5f, .5f);
        rt.anchorMin = new Vector2(.5f, .5f);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x * .5f, rt.sizeDelta.y * .5f);
        ui_component.AddComponent<Image>();
        ui_component.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(Resources.Load("Prefabs/" + name)),
            new Rect(0, 0, 128, 128), new Vector2()); // this needs to be configured
                                                      // size of textures can differ from sprite to sprite
        menu_objects.Add(ui_component);
    }

    private void display_items() {
        // For now, given start_location
        // near the left of the screen
        // we will place each item prefab
        // texture apart based on offset
        // from each subsequent texture
        // for blank items, we use none

        // Removal of the item from the list
        // below should shift all indexes to
        // the left - therefore it should
        // always be fine to iterate over
        // the list from left to right

        start_location_x = equipped_icon.position.x;
        start_location_y = equipped_icon.position.y;
        start_location_z = HUD.GetComponent<RectTransform>().transform.position.z;

        int num_items = 0;
        foreach (string i in inventory) {
            set_item_menu_hud(i, num_items);
            ++num_items;
        }
        for (; num_items < max_items; ++num_items) {
            set_item_menu_hud(none.name, num_items);
        }

        if (desired_equipped == "") {
            current_inventory_index = 0;
        } else {
            current_inventory_index = desired_equipped_index;
        }
    }

    private void toggle_item_menu() {
        if (Input.GetButtonDown("Y") && !item_menu_on) {
            Time.timeScale = 0; // pause game upon entering
                                // menu

            display_items();
            select_item_on_menu();
            item_menu_on = true;
        } else if (Input.GetButtonDown("Y") && item_menu_on) {
            foreach (GameObject g in menu_objects) {
                Destroy(g);
            }
            menu_objects.Clear();
            Destroy(highlighter);
            item_menu_on = false;

            if (desired_equipped == "") {
                desired_equipped_index = current_inventory_index = -1; // for consistency
                                                               // we don't do anything
                                                               // with -1 case yet
            }

            Time.timeScale = 1; // start game upon exiting
                                // menu
        }
    }

    private void cycle_through_items() {
        if (!item_menu_on) {
            return;
        }

        if (Input.GetAxis("D-PadXAxis") > 0 && !switching) {
            deselect_item_on_menu();

            if (current_inventory_index + 1 == max_items) {
                current_inventory_index = 0;
            } else {
                ++current_inventory_index;
            }

            select_item_on_menu();

            switching = true;
        } else if (Input.GetAxis("D-PadXAxis") < 0 && !switching) {
            deselect_item_on_menu();

            if (current_inventory_index - 1 < 0) {
                current_inventory_index = max_items - 1;
            } else {
                --current_inventory_index;
            }

            select_item_on_menu();

            switching = true;
        } else if (Input.GetAxis("D-PadXAxis") == 0 && switching) {
            switching = false;
        }
    }

    private void equip_item() {
        if (!item_menu_on) {
            return;
        }

        if (Input.GetButtonDown("A")) {
            if (inventory.Count != 0 && current_inventory_index < inventory.Count) {
                desired_equipped = inventory[current_inventory_index];
                desired_equipped_index = current_inventory_index;

                if (equipped != null && desired_equipped != equipped.name) {
                    Destroy(equipped);
                }
            }

            set_ammo_type(desired_equipped);
        }
    }

    private void set_ammo_type(string weapon_name) {
        switch (weapon_name) {
            case "Raygun": {
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
        if (!item_menu_on) {
            return;
        }

        if (Input.GetButtonDown("B")) {
            desired_equipped = "";
            Destroy(equipped);
            set_ammo_none();
            set_equipped_none();
        }
    }

    private void drop_item() {
        if (!item_menu_on) {
            return;
        }

        if (Input.GetButtonDown("X")) {
            if (current_inventory_index < inventory.Count) {
                string dropped = inventory[current_inventory_index];

                if (desired_equipped_index == current_inventory_index) {
                    desired_equipped = "";
                    desired_equipped_index = -1; // consistency

                    // Unequip the item effectively
                    // If it is the one that we are dropping
                    set_ammo_none();
                    set_equipped_none();
                    Destroy(equipped);
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

                foreach (GameObject g in menu_objects) {
                    Destroy(g);
                }
                menu_objects.Clear();
                display_items();

                // instantiate the object under the player
                GameObject item = (GameObject)Instantiate(Resources.Load("Prefabs/" + dropped));
                item.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                if (item.GetComponent<BasicWeapon>().GetWeaponType() == Weapon.WEAPON_TYPE.RANGE) {
                    item.GetComponent<HeldAmmo>().ammo_amount = ammo_inventory[dropped].ammo_amount;
                    ammo_inventory.Remove(dropped);
                }
            }
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
        ammo_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(Resources.Load("Prefabs/" + c.GetAmmoType().name)),
            new Rect(0, 0, 128, 128), new Vector2());
    }

    public void update_equipped() {
        switch (desired_equipped) {
            case "Raygun": {
                    GameObject raygun = Instantiate(Resources.Load("Prefabs/" + desired_equipped)) as GameObject;
                    DefaultInitEquipped(raygun);

                    tps.current_weapon_range = raygun.GetComponent<BasicWeapon>().range;
                    equipped = raygun;
                }
                break;
            case "Sword": {
                    GameObject sword = Instantiate(Resources.Load("Prefabs/" + desired_equipped)) as GameObject;
                    DefaultInitEquipped(sword);

                    sword.transform.Rotate(0, 90, 0);
                    tps.current_weapon_range = sword.GetComponent<BasicWeapon>().range;
                    equipped = sword;
                }
                break;
            default: {
                    equipped = null;
                    crosshair.GetComponent<AimingSystem>().enabled = false;
                }
                break;
        }

        // Update GUI elements (later through event system)
        if (equipped != null) {
            equipped_status.gameObject.GetComponent<Text>().text = equipped.name + " equipped";
            equipped_status.gameObject.GetComponent<Text>().color = new Color(148, 0, 211);

            equipped_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(Resources.Load("Prefabs/" + equipped.name)),
            new Rect(0, 0, 128, 128), new Vector2());

            crosshair.GetComponent<AimingSystem>().enabled = true;
        }
    }

    private void set_equipped_none() {
        equipped_status.gameObject.GetComponent<Text>().text = "None equipped";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(255, 255, 0);


        equipped_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(none),
            new Rect(0, 0, 128, 128), new Vector2());

        crosshair.GetComponent<AimingSystem>().enabled = false;
    }

    private void set_ammo_none() {
        c.SetAmmoAmount(0);
        c.SetAmmoType(null);

        ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo not set ";
        ammo_remaining.gameObject.GetComponent<Text>().color = new Color(255, 255, 0);

        ammo_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(none),
            new Rect(0, 0, 128, 128), new Vector2());
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

    void DefaultInitEquipped(GameObject new_equipped) {
        new_equipped.name = new_equipped.name.Substring(0, new_equipped.name.LastIndexOf("("));
        Destroy(new_equipped.GetComponent<Rigidbody>());
        Destroy(new_equipped.GetComponent<BoxCollider>());
        Transform right_arm = transform.Find("RightArm");
        Vector3 pos = right_arm.transform.position;
        pos += right_arm.transform.forward * .3f;
        pos += right_arm.transform.up * .2f;
        new_equipped.transform.position = pos;
        new_equipped.transform.parent = right_arm;
        Vector3 direction = Vector3.RotateTowards(new_equipped.transform.forward, transform.forward, Mathf.PI, 0);
        new_equipped.transform.rotation = Quaternion.LookRotation(direction);
    }
}
