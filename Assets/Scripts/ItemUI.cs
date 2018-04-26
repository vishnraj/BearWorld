using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InputEvents;

public class ItemUI : MonoBehaviour {
    public GameObject event_manager;
    public GameObject player;
    public GameObject none;
    public Sprite selection;

    public int max_items;
    public float offset_x;

    bool item_menu_on = false; // this is going to change to a state
    bool switching = false;
    float start_location_x;
    float start_location_y;
    float start_location_z;

    List<GameObject> menu_objects;
    List<string> inventory;

    Inventory inventory_system;
    GameObject highlighter;

    // Use this for initialization
    void Start () {
        Transform equipped_icon = transform.parent.Find("EquippedIcon");

        menu_objects = new List<GameObject>();
        inventory = player.GetComponent<Inventory>().get_inventory();

        inventory_system = player.GetComponent<Inventory>();

        start_location_x = equipped_icon.position.x;
        start_location_y = equipped_icon.position.y;
        start_location_z = transform.parent.GetComponent<RectTransform>().transform.position.z;

        event_manager.GetComponent<InputManager>().publisher.InputEvent += TestCallBack;
        event_manager.GetComponent<InputManager>().publisher.InputEvent += MenuCallback;
    }
	
	// Update is called once per frame
	void Update () {
		if (item_menu_on) {
            cycle_through_items();
        }
	}

    void TestCallBack(object sender, InputEvents.INPUT_EVENT e) {
        Debug.Log("here");
    }

    void MenuCallback(object sender, InputEvents.INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.START :
                toggle_item_menu();
                break;
            case INPUT_EVENT.X: {
                    inventory_system.drop_item();
                    refresh_menu();
                }
                break;
            default :
                break;
        }
    }

    private void refresh_menu() {
        if (!item_menu_on)
            return;

        foreach (GameObject g in menu_objects) {
            Destroy(g);
        }
        menu_objects.Clear();
        display_items();
    }

    private void toggle_item_menu() {
        if (!item_menu_on) {
            Time.timeScale = 0; // pause game upon entering
                                // menu

            display_items();
            select_item_on_menu();
            item_menu_on = true;
        } else { 
            foreach (GameObject g in menu_objects) {
                Destroy(g);
            }
            menu_objects.Clear();
            Destroy(highlighter);
            item_menu_on = false;

            Time.timeScale = 1; // start game upon exiting
                                // menu
        }
    }

    private void deselect_item_on_menu() {
        menu_objects[inventory_system.get_current_index()].transform.SetParent(transform.parent.transform);
    }

    private void select_item_on_menu() {
        GameObject selected_ui_component = menu_objects[inventory_system.get_current_index()]; // has to be in here

        RectTransform rt = selected_ui_component.GetComponent<RectTransform>();
        RectTransform rt_highlight;

        if (highlighter == null) {
            highlighter = new GameObject();
            highlighter.transform.SetParent(transform.parent.transform, false);
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

        int num_items = 0;
        foreach (string i in inventory) {
            set_item_menu_hud(i, num_items);
            ++num_items;
        }
        for (; num_items < max_items; ++num_items) {
            set_item_menu_hud(none.name, num_items);
        }

        inventory_system.set_current_index();
    }

    private void set_item_menu_hud(string name, int current_index) {
        // create the object 
        // add rect transform and set the position
        // add image and set the sprite
        // set transform parent to UI
        // add it to menu_objects for later destruction

        GameObject ui_component = new GameObject();
        ui_component.transform.SetParent(transform.parent, false);
        ui_component.AddComponent<RectTransform>();
        RectTransform rt = ui_component.GetComponent<RectTransform>();
        rt.position = new Vector3(start_location_x + (current_index + 1) * offset_x, start_location_y, start_location_z);
        rt.anchorMax = new Vector2(.5f, .5f);
        rt.anchorMin = new Vector2(.5f, .5f);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x * .5f, rt.sizeDelta.y * .5f);
        ui_component.AddComponent<Image>();

        Sprite s = Resources.Load<Sprite>("Sprites/" + name);
        if (s != null) {
            ui_component.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
        }

        menu_objects.Add(ui_component);
    }

    private void cycle_through_items() {
        if (!item_menu_on) {
            return;
        }

        if (Input.GetAxis("D-PadXAxis") > 0 && !switching) {
            deselect_item_on_menu();

            inventory_system.increment_current_index();

            select_item_on_menu();

            switching = true;
        } else if (Input.GetAxis("D-PadXAxis") < 0 && !switching) {
            deselect_item_on_menu();

            inventory_system.decrement_current_index();

            select_item_on_menu();

            switching = true;
        } else if (Input.GetAxis("D-PadXAxis") == 0 && switching) {
            switching = false;
        }
    }
}
