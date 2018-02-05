using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Will determine how items are picked up and assigned 
// to player later as well as notifying other systems
// about these items 
public class ItemSystem : MonoBehaviour
{
    public GameObject desired_weapon;
    public GameObject equipped;
    public GameObject desired_ammo_type;
    public int desired_ammo_amount;
    public GameObject HUD;

    Transform equipped_icon;
    Transform equipped_status;
    Transform ammo_icon;
    Transform ammo_remaining;
    Transform crosshair;
    bool using_item = false;

    PlayerAttackController pac;
    ThirdPersonTargetingSystem tps;
    BasicCharacter c;

    // Use this for initialization
    void Start()
    {
        // Everything here requires on hud for the most part
        // so it's intialized in start, but HUD stuff should be
        // intialized in Awake

        tps = GetComponent<ThirdPersonTargetingSystem>();
        pac = GetComponent<PlayerAttackController>();
        c = GetComponent<BasicCharacter>();

        equipped_icon = HUD.transform.Find("EquippedIcon");
        equipped_status = HUD.transform.Find("EquippedStatus");

        ammo_icon = HUD.transform.Find("AmmoIcon");
        ammo_remaining = HUD.transform.Find("AmmoRemaining");
        crosshair = HUD.transform.Find("Crosshair");

        UpdateEquippedToSelected(); // for now it is first set here (eventually when an item is picked up and then later when 
        // it is picked and selected from inventory list).
        // After first selection it is only ever then set to selected when it is put away (using b)

        UpdateAmmoIcon(); // for now updated here but later will be when selected from inventory list or when picked up

        UpdateEquippedToUsing(); // should be set in a different script when l-trigger is pushed once - event system
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmoAmountOnGUI(); // this and other GUI Components should get updated
                                 // using the Event System, but for now we are updating
                                 // it here vs from the component because items can no longer
                                 // know that they are owned by a player - this has now led
                                 // to it having to be "updated" on every call to update
                                 // which leads to the better thinking that it and other GUI elements
                                 // not be updated by the components they track, directly, but rather
                                 // by the event system (input, or messages from a component can update,
                                 // though likely these must be very player specific)
    }

    public void UpdateAmmoAmountOnGUI()
    {
        ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo Amount: " + c.GetAmmoAmount().ToString();

        if (c.GetAmmoAmount() > 5)
        {
            ammo_remaining.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
        }
        else
        {
            ammo_remaining.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
        }
    }

    public void UpdateAmmoIcon()
    {
        ammo_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(desired_ammo_type),
            new Rect(0, 0, 128, 128), new Vector2());

        UpdateAmmoAmountOnGUI();
    }

    public void UpdateEquippedToUsing()
    {
        switch (desired_weapon.name)
        {
            case "Raygun": {
                    GameObject raygun = Instantiate(desired_weapon) as GameObject;
                    Transform right_arm = transform.Find("RightArm");
                    Vector3 pos = right_arm.position;
                    pos.x += .1f;
                    pos.y += .3f;
                    pos.z += .4f;
                    raygun.transform.position = pos;
                    raygun.transform.parent = right_arm;

                    tps.current_weapon_range = 100.0f;

                    ActivateItem(raygun);
                    c.SetAmmoType(desired_ammo_type);
                    c.SetAmmoAmount(desired_ammo_amount);
                }
                break;
            case "Sword": {
                    GameObject sword = Instantiate(desired_weapon) as GameObject;
                    Transform right_arm = transform.Find("RightArm");
                    Vector3 pos = right_arm.position;
                    pos.x += 0f;
                    pos.y += .02f;
                    pos.z += .3f;
                    sword.transform.position = pos;
                    sword.transform.Rotate(90, 90, 0);

                    sword.transform.parent = right_arm;

                    tps.current_weapon_range = 30.0f;

                    ActivateItem(sword);
                }
                break;
            default:
                break;
        }

        crosshair.GetComponent<AimingSystem>().enabled = true;
        equipped_status.gameObject.GetComponent<Text>().text = "Equipped Status: Using";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
    }

    void ActivateItem(GameObject o) {
        pac.weapon = o.GetComponent<BasicWeapon>();
        pac.weapon.SetCharacter(c);
        pac.weapon.enabled = true;
        equipped = o;
        using_item = true;
    }

    public void UpdateEquippedToSelected()
    {
        equipped_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(desired_weapon),
            new Rect(0, 0, 128, 128), new Vector2());

        equipped_status.gameObject.GetComponent<Text>().text = "Equipped Status: Selected";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
    }
}
