using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : BasicHealth
{
    public GameObject player; // this is only for the icon
    public GameObject HUD;

    GameObject game_over;
    Transform player_icon;
    Transform health_remaining;

    // Use this for initialization
    void Start()
    {
        player_icon = HUD.transform.Find("PlayerIcon");
        health_remaining = HUD.transform.Find("HealthRemaining");

        Sprite s = Resources.Load<Sprite>("Sprites/" + player.name);
        if (s != null) {
            player_icon.gameObject.GetComponent<Image>().sprite = s;
        } else {
            Debug.LogError("We had an issue generating the asset preview");
        }

        UpdateHealthRemainingOnGUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthRemainingOnGUI();

        if (health <= 0) {
            CreateGameOverOnGUI();
            Destroy(gameObject);
        }
    }

    public void UpdateHealthRemainingOnGUI()
    {
        if (health > 5) {
            health_remaining.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
        } else {
            health_remaining.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
        }

        if (health < 0) {
            health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: 0";
            return;
        }

        health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: " + health.ToString();
    }

    void CreateGameOverOnGUI() {
        game_over = new GameObject();
        game_over.name = "Game Over";
        game_over.AddComponent<Text>().text = health.ToString();
        game_over.layer = 5;

        Text game_over_text = game_over.GetComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        game_over_text.fontStyle = FontStyle.Bold;
        game_over_text.font = ArialFont;
        game_over_text.fontSize = 100;
        game_over_text.enabled = true;
        game_over_text.color = new Color(255, 0, 0);
        game_over_text.text = "GAME OVER";

        game_over.transform.SetParent(HUD.transform);

        RectTransform game_over_rect = game_over.GetComponent<RectTransform>();
        game_over_rect.sizeDelta = new Vector2(1000, 200);

        RectTransform hud_rect = HUD.GetComponent<RectTransform>();
        Vector2 game_over_position = new Vector2(hud_rect.rect.x + hud_rect.sizeDelta.x * .6f, hud_rect.rect.y + hud_rect.sizeDelta.y * .5f);

        game_over_rect.anchoredPosition = game_over_position;
    }
}
