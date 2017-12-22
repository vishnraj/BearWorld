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
    public GameObject equipped;
    public GameObject current_ammo_type;
    public int current_ammo_amount;

    public GameObject HUD;
    Transform equipped_icon;
    Transform equipped_status;
    Transform ammo_icon;
    Transform ammo_remaining;
    Transform crosshair;

    ThirdPersonTargetingSystem tps;

    // Use this for initialization
    void Start()
    {
        tps = GetComponentInChildren<ThirdPersonTargetingSystem>();

        equipped_icon = HUD.transform.Find("EquippedIcon");
        equipped_status = HUD.transform.Find("EquippedStatus");

        ammo_icon = HUD.transform.Find("AmmoIcon");
        ammo_remaining = HUD.transform.Find("AmmoRemaining");
        crosshair = HUD.transform.Find("Crosshair");

        UpdateEquippedToSelected(); // for now it is first set here (eventually when an item is picked up and then later when 
        // it is picked and selected from inventory list).
        // After first selection it is only ever then set to selected when it is put away (using b)

        UpdateAmmoIcon(); // for now updated here but later will be when selected from inventory list or when picked up

        UpdateEquippedToUsing(); // should be set in a different script when l-trigger is pushed once
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateAmmoAmountOnGUI()
    {
        ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo Amount: " + current_ammo_amount.ToString();

        if (current_ammo_amount > 5)
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
        ammo_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(current_ammo_type),
            new Rect(0, 0, 128, 128), new Vector2());

        UpdateAmmoAmountOnGUI();
    }

    public void UpdateEquippedToUsing()
    {
        switch (equipped.name)
        {
            case "Raygun":
                GameObject raygun = Instantiate(equipped) as GameObject;
                Transform right_arm = transform.Find("RightArm");

                Vector3 pos = right_arm.position;
                pos.x += .1f;
                pos.y += .3f;
                pos.z += .4f;
                raygun.transform.position = pos;

                raygun.transform.parent = right_arm;

                tps.current_weapon_range = 100.0f;

                crosshair.GetComponent<AimingSystem>().enabled = true;
                break;

            default:
                break;
        }

        equipped_status.gameObject.GetComponent<Text>().text = "Equipped Status: Using";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
    }

    public void UpdateEquippedToSelected()
    {
        equipped_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(equipped),
            new Rect(0, 0, 128, 128), new Vector2());

        equipped_status.gameObject.GetComponent<Text>().text = "Equipped Status: Selected";
        equipped_status.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
    }
}
