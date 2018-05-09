using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyHealth : BasicHealth
{
    public List<string> random_drop_list;

    Camera cam; // this shittiness will be removed eventually, as we update more things to use events - eventually the piece that needs this
                // will be moved into the UI code, so this will not have to be used here anymore to create UI elements
    GameObject HUD;
    GameObject player;
    GameObject enemies;
    GameObject health_remaining;

    // Use this for initialization
    void Start()
    {
        // initialize objects because this will exist in a prefabs
        HUD = GameObject.Find("HUD");
        player = GameObject.Find("Bear"); // things related to player should be communicated via messages
        enemies = GameObject.Find("Enemies");
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        enemies.GetComponent<EnemyTracker>().AddEnemy(gameObject);

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
                health_remaining == null) {
                CreateHealthRemainingOnGUI();
            }

            if (player.GetComponent<ThirdPersonTargetingSystem>().locked_on &&
                health_remaining != null) {
                UpdateHealthRemainingOnGUI();
            }
        }
        
        if (health <= 0) {
            if (GetComponent<EnemyExplosion>() != null) {
                // from this point (unless we have reform script)
                // we will delagate death of the enemy to the
                // enemy explosion script
                Destroy(health_remaining);
                enemies.GetComponent<EnemyTracker>().RemoveEnemy(gameObject); // we don't want this to be tracked (unless reformed)
                enabled = false;
                return;
            }

            if (health_remaining != null) {
                health_remaining.transform.SetParent(null);
            }
            Destroy(health_remaining);

            enemies.GetComponent<EnemyTracker>().RemoveEnemy(gameObject);

            if (random_drop_list.Count != 0) {
                Items.DropFactory f = new Items.DropFactory();
                f.Drop(random_drop_list, transform.position);
            }

            Destroy(gameObject);
        }
    }

    void UpdateHealthRemainingOnGUI() {
        Text health_text = health_remaining.GetComponent<Text>();
        health_text.text = health.ToString();

        Vector2 enemy_screen_point = cam.WorldToViewportPoint(transform.position);
        RectTransform hud_rect = HUD.GetComponent<RectTransform>();
        Vector2 enemy_health_position = new Vector2(
        ((enemy_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.46f)),
        ((enemy_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.53f)));

        health_remaining.GetComponent<RectTransform>().anchoredPosition = enemy_health_position;
    }

    void CreateHealthRemainingOnGUI() {
        health_remaining = new GameObject();
        health_remaining.name = "Health Remaining " + name;
        health_remaining.AddComponent<Text>().text = health.ToString();
        health_remaining.layer = 5;

        Text health_text = health_remaining.GetComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        health_text.fontStyle = FontStyle.Bold;
        health_text.font = ArialFont;
        health_text.fontSize = 20;
        health_text.enabled = true;
        health_text.color = new Color(255, 0, 0);

        health_remaining.transform.SetParent(HUD.transform);
    }
}
