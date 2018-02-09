using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Utility;

// Used to deliver target(s) to other systems
public class AimingSystem : MonoBehaviour
{
    public GameObject crosshair;
    public Camera cam;
    public GameObject player;
    public Ray ray;

    public Sprite default_reticle;
    public Sprite target_reticle;

    ThirdPersonTargetingSystem tps;
    BasicCharacter c;
    Searching search_tool;

    // Use this for initialization
    void Start()
    {
        tps = player.GetComponent<ThirdPersonTargetingSystem>();
        c = player.GetComponent<BasicCharacter>();
        search_tool = new Searching();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        ray = cam.ScreenPointToRay(crosshair.GetComponent<RectTransform>().position);
        tps.direction = ray.GetPoint(tps.current_weapon_range);

        if (Physics.Raycast(ray, out hit, tps.current_weapon_range))
        {
            if (hit.collider.tag == "Enemy" && !tps.locked_on)
            {
                crosshair.GetComponent<Image>().sprite = target_reticle;
                tps.target = search_tool.FindComponentUpHierarchy<EnemyHealth>(hit.collider.gameObject.transform);
            }
            else if (hit.collider.tag == "Enemy" && tps.locked_on)
            {
                crosshair.GetComponent<Image>().sprite = target_reticle;
            }
            else
            {
                SetDefaults();
            }
        }
        else
        {
            SetDefaults();
        }
    }

    void OnEnable()
    {
        if (player != null) {
            tps = player.GetComponent<ThirdPersonTargetingSystem>();
            crosshair.GetComponent<Image>().enabled = true;
            SetDefaults();
        }
    }

    private void OnDisable() {
        crosshair.GetComponent<Image>().enabled = false;
    }

    void SetDefaults()
    {
        crosshair.GetComponent<Image>().sprite = default_reticle;

        if (!tps.locked_on)
        {
            tps.target = null;
        }
    }
}
