using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public GameObject player;
    public float health;

    public GameObject HUD;
    Transform player_icon;
    Transform health_remaining;

    // Use this for initialization
    void Start()
    {
        player_icon = HUD.transform.Find("PlayerIcon");
        health_remaining = HUD.transform.Find("HealthRemaining");

        player_icon.gameObject.GetComponent<Image>().sprite = Sprite.Create(AssetPreview.GetAssetPreview(player),
            new Rect(0, 0, 128, 128), new Vector2());

        UpdateHealthRemainingOnGUI();
    }



    // Update is called once per frame
    void Update()
    {
        UpdateHealthRemainingOnGUI();

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateHealthRemainingOnGUI()
    {
        health_remaining.gameObject.GetComponent<Text>().text = "Health Remaining: " + health.ToString();

        if (health > 5)
        {
            health_remaining.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
        }
        else
        {
            health_remaining.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
        }
    }
}
