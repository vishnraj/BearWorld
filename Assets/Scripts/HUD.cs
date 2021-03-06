﻿using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using PlayerHealthEvents;
using InventoryEvents;
using TargetingEvents;
using EnemyHealthEvents;

public class HUD : MonoBehaviour {
    public GameObject player;
    public Camera cam;
    public GameObject event_manager;

    GameObject game_over;
    Transform player_icon;
    Transform health_remaining;

    Transform targeting_status;
    Transform equipped_icon;
    Transform equipped_status;
    Transform ammo_icon;
    Transform ammo_remaining;

    Dictionary<int, GameObject> enemy_health_remaining;

    Color blue = new Color(0, 0, 255);
    Color red = new Color(255, 0, 0);
    Color yellow = new Color(255, 255, 0);
    Color lime_green = new Color(.49f, 1f, 0f);
    Color purple = new Color(148, 0, 211);

    // Use this for initialization
    void Start () {
        player_icon = transform.Find("PlayerIcon");

        Sprite s = Resources.Load<Sprite>("Sprites/" + player.name);
        if (s != null) {
            player_icon.gameObject.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
        }

        health_remaining = transform.Find("HealthRemaining");

        targeting_status = transform.Find("TargetingStatus");

        equipped_icon = transform.Find("EquippedIcon");
        equipped_status = transform.Find("EquippedStatus");

        ammo_icon = transform.Find("AmmoIcon");
        ammo_remaining = transform.Find("AmmoRemaining");

        enemy_health_remaining = new Dictionary<int, GameObject>();

        event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent += PlayerHealthEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().inventory_publisher.InventoryEvent += InventoryEventCallback;
        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventCallback;
        event_manager.GetComponent<ComponentEventManager>().enemy_health_publisher.EnemyHealthEvent += EnemyHealthEventsCallback;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void PlayerHealthEventsCallback(float health, PLAYER_HEALTH_EVENT e) {
        switch (e) {
            case PLAYER_HEALTH_EVENT.INIT: {
                    health_remaining.gameObject.GetComponent<Text>().color = blue; // default color
                    health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: " + health.ToString();
                }
                break;
            case PLAYER_HEALTH_EVENT.UPDATE: {
                    Color health_color = health_remaining.gameObject.GetComponent<Text>().color;

                    if (health > 5 && health_color != blue) {
                        health_remaining.gameObject.GetComponent<Text>().color = blue;
                    } else if (health <= 5 && health_color != red) {
                        health_remaining.gameObject.GetComponent<Text>().color = red;
                    }

                    health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: " + health.ToString();
                }
                break;
            case PLAYER_HEALTH_EVENT.DEAD: {
                    health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: 0";
                    CreateGameOver();
                }
                break;
            default:
                break;
        }
    }

    void EnemyHealthEventsCallback(EnemyHealthData data, ENEMY_HEALTH_EVENT e) {
        switch (e) {
            case ENEMY_HEALTH_EVENT.INIT: {
                    if (!enemy_health_remaining.ContainsKey(data.id)) {
                        CreateEnemyHealthRemainingOnGUI(data.id, data.health);
                    }
                }
                break;
            case ENEMY_HEALTH_EVENT.UPDATE: {
                    if (!enemy_health_remaining.ContainsKey(data.id)) {
                        Debug.Log("Error: ID " + data.id.ToString() + " not valid.");
                        return;
                    }

                    UpdateEnemyHealthRemaining(enemy_health_remaining[data.id], data.pos,
                        data.health);
                }
                break;
            case ENEMY_HEALTH_EVENT.DESTROY:
            case ENEMY_HEALTH_EVENT.FREE: {
                    if (enemy_health_remaining.ContainsKey(data.id)) {
                        Destroy(enemy_health_remaining[data.id]);
                        enemy_health_remaining.Remove(data.id);
                    }
                }
                break;
            default:
                break;
        }
    }

    void InventoryEventCallback(object data, INVENTORY_EVENT e) {
        switch (e) {
            case INVENTORY_EVENT.EQUIP: {
                    GameObject item = (GameObject)data;
                    if (item == null || item.GetComponent<BasicWeapon>() == null) {
                        // this equip event is not relevant to this component
                        return;
                    }

                    string item_name = item.GetComponent<BasicWeapon>().GetWeaponName();

                    equipped_status.gameObject.GetComponent<Text>().text = item_name + " equipped";
                    equipped_status.gameObject.GetComponent<Text>().color = purple;

                    Sprite s = Resources.Load<Sprite>("Sprites/" + item_name);
                    if (s != null) {
                        equipped_icon.gameObject.GetComponent<Image>().sprite = s;
                    } else {
                        Debug.LogError("We had an issue generating the asset preview");
                    }
                }
                break;
            case INVENTORY_EVENT.INIT:
            case INVENTORY_EVENT.UNEQUIP: {
                    equipped_status.gameObject.GetComponent<Text>().text = "None equipped";
                    equipped_status.gameObject.GetComponent<Text>().color = yellow;

                    Sprite s = Resources.Load<Sprite>("Sprites/Blank");
                    if (s != null) {
                        equipped_icon.gameObject.GetComponent<Image>().sprite = s;
                    } else {
                        Debug.LogError("We had an issue generating the asset preview");
                    }
                }
                goto case INVENTORY_EVENT.UNSET_AMMO;
            case INVENTORY_EVENT.SET_AMMO: {
                    string ammo_name = (string)data;
                    if (ammo_name == null) {
                        return;
                    }

                    Sprite s = Resources.Load<Sprite>("Sprites/" + ammo_name);
                    if (s != null) {
                        ammo_icon.gameObject.GetComponent<Image>().sprite = s;
                    } else {
                        Debug.LogError("We had an issue generating the asset preview");
                    }
                }
                break;
            case INVENTORY_EVENT.UNSET_AMMO: {
                    ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo not set";
                    ammo_remaining.gameObject.GetComponent<Text>().color = yellow;

                    Sprite s = Resources.Load<Sprite>("Sprites/Blank");
                    if (s != null) {
                        ammo_icon.gameObject.GetComponent<Image>().sprite = s;
                    } else {
                        Debug.LogError("We had an issue generating the asset preview");
                    }
                }
                break;
            case INVENTORY_EVENT.UPDATE_AMMO_AMOUNT: {
                    float ammo_amount = (float)data;
                    Color ammo_remaining_color = ammo_remaining.gameObject.GetComponent<Text>().color;

                    if (ammo_amount > 5 && ammo_remaining_color != purple) {
                        ammo_remaining.gameObject.GetComponent<Text>().color = purple;
                    } else if (ammo_amount <= 5 && ammo_remaining_color != red) {
                        ammo_remaining.gameObject.GetComponent<Text>().color = red;
                    }

                    ammo_remaining.gameObject.GetComponent<Text>().text = "Ammo Amount: " + ammo_amount.ToString();
                }
                break;
            default:
                break;
        }
    }

    void TargetingEventCallback(GameObject target, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.INIT:
            case TARGETING_EVENT.FREE: {
                    targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Can't Lock";
                    targeting_status.gameObject.GetComponent<Text>().color = blue;
                }
                break;
            case TARGETING_EVENT.CAN_LOCK: {
                    targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Can Lock";
                    targeting_status.gameObject.GetComponent<Text>().color = lime_green;
                }
                break;
            case TARGETING_EVENT.LOCK_ON: {
                    targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Locked";
                    targeting_status.gameObject.GetComponent<Text>().color = red;
                }
                break;
            default:
                break;
        }
    }

    void UpdateEnemyHealthRemaining(GameObject enemy_health_ui, Vector3 pos, float health) {
        Text health_text = enemy_health_ui.GetComponent<Text>();
        health_text.text = health.ToString();

        Vector2 enemy_screen_point = cam.WorldToViewportPoint(pos);
        RectTransform hud_rect = GetComponent<RectTransform>();
        Vector2 enemy_health_position = new Vector2(
        ((enemy_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.46f)),
        ((enemy_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.53f)));

        enemy_health_ui.GetComponent<RectTransform>().anchoredPosition = enemy_health_position;
    }

    void CreateEnemyHealthRemainingOnGUI(int id, float health) {
        GameObject enemy_health_ui = new GameObject();
        enemy_health_ui.name = "Enemy Health " + id.ToString();
        enemy_health_ui.AddComponent<Text>().text = health.ToString();
        enemy_health_ui.layer = 5;

        Text health_text = enemy_health_ui.GetComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        health_text.fontStyle = FontStyle.Bold;
        health_text.font = ArialFont;
        health_text.fontSize = 20;
        health_text.enabled = true;
        health_text.color = red;

        enemy_health_ui.transform.SetParent(transform);
        enemy_health_remaining.Add(id, enemy_health_ui);
    }

    void CreateGameOver() {
        game_over = new GameObject();
        game_over.name = "Game Over";
        game_over.AddComponent<Text>();
        game_over.layer = 5;

        Text game_over_text = game_over.GetComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        game_over_text.fontStyle = FontStyle.Bold;
        game_over_text.font = ArialFont;
        game_over_text.fontSize = 100;
        game_over_text.enabled = true;
        game_over_text.color = new Color(255, 0, 0);
        game_over_text.text = "GAME OVER";

        game_over.transform.SetParent(transform);

        RectTransform game_over_rect = game_over.GetComponent<RectTransform>();
        game_over_rect.sizeDelta = new Vector2(1000, 200);

        RectTransform hud_rect = GetComponent<RectTransform>();
        Vector2 game_over_position = new Vector2(hud_rect.rect.x + hud_rect.sizeDelta.x * .6f, hud_rect.rect.y + hud_rect.sizeDelta.y * .5f);

        game_over_rect.anchoredPosition = game_over_position;

        GameObject button = null;
        button = Instantiate(Resources.Load("Prefabs/ToMainMenu"), transform.position, new Quaternion()) as GameObject;
        button.transform.SetParent(transform);
        button.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(300, -150, 0);
    }
}
