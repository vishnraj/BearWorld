using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    GameObject HUD;
    GameObject player;
    public float health = 10;
    GameObject health_remaining;

    // Use this for initialization
    void Start()
    {
        // initialize HUD and player
        HUD = GameObject.Find("HUD");
        player = GameObject.Find("Bear");
        health_remaining = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null) {
            // This should all be event driven - detect if left trigger is held down
            // if so, the event system will update the GUI to refelct the health of
            // each enemy in the game

            if (!player.GetComponent<ThirdPersonTargetingSystem>().locked_on &&
            health_remaining != null) {
                health_remaining.transform.SetParent(null);
                Destroy(health_remaining);
                health_remaining = null;
            }

            if (player.GetComponent<ThirdPersonTargetingSystem>().locked_on &&
                health_remaining == null)
                CreateHealthRemainingOnGUI();

            if (player.GetComponent<ThirdPersonTargetingSystem>().locked_on &&
                health_remaining != null)
                UpdateHealthRemainingOnGUI();
        }
        
        if (health <= 0) {
            if (health_remaining != null) {
                health_remaining.transform.SetParent(null);
            }
            Destroy(health_remaining);
            Destroy(gameObject);
        }
    }

    void UpdateHealthRemainingOnGUI() {
        Text health_text = health_remaining.GetComponent<Text>();
        health_text.text = health.ToString();

        Vector2 enemy_screen_point = player.GetComponent<ThirdPersonTargetingSystem>().main_camera.WorldToViewportPoint(transform.position);
        RectTransform hud_rect = HUD.GetComponent<RectTransform>();
        Vector2 enemy_health_position = new Vector2(
        ((enemy_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.46f)),
        ((enemy_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.53f)));

        health_remaining.GetComponent<RectTransform>().anchoredPosition = enemy_health_position;
    }

    void CreateHealthRemainingOnGUI() {
        health_remaining = new GameObject();
        health_remaining.AddComponent<Text>().text = health.ToString();
        health_remaining.layer = 5;

        Text health_text = health_remaining.GetComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        health_text.fontStyle = FontStyle.Bold;
        health_text.font = ArialFont;
        health_text.fontSize = 20;
        health_text.enabled = true;
        health_text.color = new Color(255, 0, 0);

        Vector2 enemy_screen_point = player.GetComponent<ThirdPersonTargetingSystem>().main_camera.WorldToViewportPoint(transform.position);
        RectTransform hud_rect = HUD.GetComponent<RectTransform>();
        Vector2 enemy_health_position = new Vector2(
        ((enemy_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.46f)),
        ((enemy_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.53f)));

        health_remaining.transform.SetParent(HUD.transform);
        health_remaining.GetComponent<RectTransform>().anchoredPosition = enemy_health_position;
    }
}
