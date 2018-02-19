using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Utility;

public class ThirdPersonTargetingSystem : MonoBehaviour
{
    public float current_weapon_range;
    public bool locked_on = false;
    public bool can_lock_on = false;
    public bool switching = false;
    public bool forward_facing = true;

    public Vector3 direction;
    public GameObject target = null;
    public GameObject HUD;
    public GameObject Enemies;
    public Camera main_camera;
    public SortedList sorted_by_chosen_direction; // targets sorted by closest distance to current target
                                                  // as well as closeness of the angle of the vector to new target
                                                  // to the angle created by the joystick's coordinates
                                                  // if there is a target, otherwise just sorts using InstanceIDs
                                                  // in ascending order
    float input_x = 0.0f;
    float input_y = 0.0f;
    float current_joystick_angle = 0.0f;
    float margin_of_error = 5.0f;

    Vector3 previous_target_pos;

    GameObject targeting_icon;
    Transform targeting_status;
    Transform crosshair;

    CloseCompare compare_distances;
    Rotation rt;
    BasicCharacter c;

    // Use this for initialization
    void Start()
    {
        targeting_status = HUD.transform.Find("TargetingStatus");
        crosshair = HUD.transform.Find("Crosshair");
        targeting_icon = new GameObject();

        // Terrible, think this should all be controlled in the crosshair, but communicated
        // from this script via event queue and message passing
        targeting_icon.AddComponent<Image>();
        targeting_icon.GetComponent<Image>().sprite = HUD.transform.Find("Crosshair").GetComponent<AimingSystem>().target_reticle;
        targeting_icon.name = "TargetingIcon";

        float desired_scale = .4f;

        previous_target_pos = new Vector3();

        targeting_icon.GetComponent<Image>().transform.localScale = new Vector3(desired_scale, desired_scale, desired_scale);

        compare_distances = new CloseCompare(this);
        sorted_by_chosen_direction = new SortedList(compare_distances);

        rt = new Rotation();
        c = GetComponent<BasicCharacter>();

        // Call once to set to default state
        UpdateTargetingStatus();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // This all needs to be handled via  state class
            // because this has become unruly garbage

            Vector3 to_target = target.transform.position - transform.position;

            if (!locked_on)
            {
                can_lock_on = true;
                UpdateTargetingStatus();
            }

            if (can_lock_on && Input.GetAxis("LeftTriggerAxis") > 0 && !locked_on)
            {
                locked_on = true;
                UpdateTargetingStatus();
                crosshair.GetComponent<Image>().enabled = false;
                crosshair.GetComponent<AimingSystem>().enabled = false;
            }
            else if (Input.GetAxis("LeftTriggerAxis") == 0 && locked_on)
            {
                locked_on = false;
                UpdateTargetingStatus();
                crosshair.GetComponent<AimingSystem>().enabled = true;
            }

            if (locked_on && to_target.magnitude <= current_weapon_range)
            {
                SetTargetOnGui();
            }
            else
            {
                DisableLockedOn();
            }

            // user input related
            if ( locked_on && (Input.GetAxis("RightJoystickX") != 0 || Input.GetAxis("RightJoystickY") != 0) && !switching )
            {
                switching = true;
                SetNewTarget();
            }

            if ( locked_on && (Input.GetAxis("RightJoystickX") == 0 && Input.GetAxis("RightJoystickY") == 0) && switching )
            {
                switching = false;
            }

            // This is stupid, but for some reason unity, after confirming that in this frame
            // this object exists, seems to show that this is null, even though state of all objects shouldn't get updated until
            // the next loop, which means we can't be here to begin with
            if (target != null) {
                c.SetTarget(target.transform.position);
                previous_target_pos = target.transform.position;
            }
        }
        else
        {
            // This type of behavior needs to be divided
            // up into states for when locked on vs not locked on
            if (locked_on) {
                List<GameObject> enemies = Enemies.GetComponent<EnemyTracker>().GetAllEnemies();
                target = GetClosestEnemy(enemies);

                if (target != null) {
                    Vector3 to_target = target.transform.position - transform.position;
                    if (to_target.magnitude <= current_weapon_range) {
                        c.SetTarget(target.transform.position);
                        SetTargetOnGui();
                        // rest of information set above, as part of targeting state,
                        // but this will be much cleaner once this section just controls
                        // the state of the class as a different class vs control flow
                    } else {
                        DisableLockedOn();
                    }
                } else {
                    DisableLockedOn();
                }
            }

            // leftover, will be done correctly with states
            // but for now this a piece that remains enabled if
            // targeting is lost and we didt't remove it earlier
            //if (targeting_icon.GetComponent<Image>().enabled) {
            //    targeting_icon.GetComponent<Image>().enabled = false;
            //}

            can_lock_on = false;
            UpdateTargetingStatus();

            c.SetTarget(direction);
        }
    }

    GameObject GetClosestEnemy(List<GameObject> enemies) {
        GameObject g_min = null;
        float min_dist = Mathf.Infinity;
        foreach (GameObject g in enemies) {
            float dist = Vector3.Distance(g.transform.position, previous_target_pos);
            if (dist < min_dist) {
                g_min = g;
                min_dist = dist;
            }
        }
        return g_min;
    }

    void DisableLockedOn() {
        locked_on = false;
        switching = false;

        crosshair.GetComponent<AimingSystem>().enabled = true;
        
        targeting_icon.GetComponent<Image>().enabled = false;

        UpdateTargetingStatus();
    }

    void UpdateTargetingStatus()
    {
        if (can_lock_on && !locked_on)
        {
            targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Can Lock";
            targeting_status.gameObject.GetComponent<Text>().color = new Color(.49f, 1f, 0f);
        }
        else if (locked_on)
        {
            targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Locked";
            targeting_status.gameObject.GetComponent<Text>().color = new Color(255, 0, 0);
        }
        else
        {
            targeting_status.gameObject.GetComponent<Text>().text = "Targeting Status: Can't Lock";
            targeting_status.gameObject.GetComponent<Text>().color = new Color(0, 0, 255);
        }
    }

    void SetTargetOnGui()
    {
        if (!targeting_icon.GetComponent<Image>().enabled)
        {
            targeting_icon.GetComponent<Image>().enabled = true;
            targeting_icon.GetComponent<Image>().sprite = HUD.transform.Find("Crosshair").GetComponent<AimingSystem>().target_reticle;
        }

        Vector2 target_screen_point = main_camera.WorldToViewportPoint(target.transform.position);
        RectTransform hud_rect = HUD.GetComponent<RectTransform>();
        Vector2 targeting_icon_position = new Vector2(
        ((target_screen_point.x * hud_rect.sizeDelta.x) - (hud_rect.sizeDelta.x * 0.5f)),
        ((target_screen_point.y * hud_rect.sizeDelta.y) - (hud_rect.sizeDelta.y * 0.5f)));

        if (!(targeting_icon.transform.parent == HUD.transform))
        {
            targeting_icon.transform.SetParent(HUD.transform);
        }

        targeting_icon.GetComponent<RectTransform>().anchoredPosition = targeting_icon_position;
    }

    void SetNewTarget()
    {
        GameObject new_target = null;

        input_x = Input.GetAxis("RightJoystickX");
        input_y = -Input.GetAxis("RightJoystickY");
        //Debug.Log("Input x: " + input_x + ", Input y: " + input_y);

		current_joystick_angle = rt.CalculateZXRotation(new Vector3(input_x, 0, input_y));
		//Debug.Log("current_joystick_angle: " + current_joystick_angle);

        sorted_by_chosen_direction.Clear();
        List<GameObject> enemies = Enemies.GetComponent<EnemyTracker>().GetAllEnemies();
        for (int i = 0; i < enemies.Count; ++i) {
			GameObject current_enemy = enemies[i];

			// here we will remove enemies from consideration, if, after being normalized to
			// an x and y (really z and x) pair that is in the player's axis, they are not
			// of the same sign for their corresponding set of inputs from the joystick
			Vector3 player_forward = new Vector3(transform.forward.x, 0, transform.forward.z);

			Vector3 to_current_enemy = current_enemy.transform.position - target.transform.position;
			float angle_to_new_target = rt.CalculateZXRotation(new Vector3(to_current_enemy.x, 0, to_current_enemy.z), player_forward);
			//Debug.Log("angle_to_new_target: " + angle_to_new_target);
				
			// Cos is calculated on y because z is effectively y
			// in our coordinate system
			float new_target_x_unit = Mathf.Sin(Mathf.Deg2Rad * angle_to_new_target);
			float new_target_y_unit = Mathf.Cos(Mathf.Deg2Rad * angle_to_new_target);

			/*
			Debug.Log("new_target_x_unit: " + new_target_x_unit);
			Debug.Log("new_target_y_unit: " + new_target_y_unit);
			*/

			bool bad_x = (new_target_x_unit > 0 && input_x < 0) || (new_target_x_unit < 0 && input_x > 0);
			bool bad_y = (new_target_y_unit > 0 && input_y < 0) || (new_target_y_unit < 0 && input_y > 0);
			if (bad_x || bad_y)
			{
				continue;
			}

            if (current_enemy.GetInstanceID() != target.GetInstanceID()) 
            {
                sorted_by_chosen_direction.Add(current_enemy, current_enemy.GetInstanceID());
            }
        }

        //Debug.Log("Sorted targets count: " + sorted_by_chosen_direction.Count);

        try {
            //Debug.Log("Selected Target ID: " + sorted_by_chosen_direction.GetByIndex(0));
			new_target = (GameObject)sorted_by_chosen_direction.GetKey(0);

			Vector3 player_forward = new Vector3(transform.forward.x, 0, transform.forward.z);

			Vector3 to_new_target = new_target.transform.position - target.transform.position;
			float angle_to_new_target = rt.CalculateZXRotation(new Vector3(to_new_target.x, 0, to_new_target.z), player_forward);
			//Debug.Log("angle_to_new_target: " + angle_to_new_target);

			// Cos is calculated on y because z is effectively y
			// in our coordinate system
			float new_target_x_unit = Mathf.Sin(Mathf.Deg2Rad * angle_to_new_target);
			float new_target_y_unit = Mathf.Cos(Mathf.Deg2Rad * angle_to_new_target);

			/*
			Debug.Log("input x: " + input_x);
			Debug.Log("input y: " + input_y);
			Debug.Log("new_target_x_unit: " + new_target_x_unit);
			Debug.Log("new_target_y_unit: " + new_target_y_unit);
			*/

			target = new_target;
        } catch (System.ArgumentOutOfRangeException) {
			/*
			Debug.Log("input x: " + input_x);
			Debug.Log("input y: " + input_y);
			*/
            //Debug.Log("All objects are out of range.");
        }
    }

    class CloseCompare : Comparer<GameObject>
    {
        ThirdPersonTargetingSystem parent;

        public CloseCompare(ThirdPersonTargetingSystem _parent)
            : base()
        {
            parent = _parent;
        }

        public override int Compare(GameObject a, GameObject b)
        {
            Vector3 to_a = a.transform.position - parent.target.transform.position;
            Vector3 to_b = b.transform.position - parent.target.transform.position;
            Vector3 player_forward = new Vector3(parent.transform.forward.x, 0, parent.transform.forward.z);

            float angle_a = parent.rt.CalculateZXRotation(new Vector3(to_a.x, 0, to_a.z), player_forward);
            float angle_b = parent.rt.CalculateZXRotation(new Vector3(to_b.x, 0, to_b.z), player_forward);

            /*
            Debug.Log("Instance ID a: " + a.gameObject.GetInstanceID());
            Debug.Log("Instance ID b: " + b.gameObject.GetInstanceID());
            Debug.Log("angle a: " + angle_a);
            Debug.Log("angle b: " + angle_b);
            */
             
            int diff_angle_a = (int) Mathf.Abs(parent.current_joystick_angle - angle_a);
            //Debug.Log("Diff angle a: " + diff_angle_a);
            int diff_angle_b = (int) Mathf.Abs(parent.current_joystick_angle - angle_b);
            //Debug.Log("Diff angle b: " + diff_angle_b);

            if (diff_angle_a < diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                //Debug.Log("distance a: " + distance_a);
                //Debug.Log("distance b: " + distance_b);

                if (diff_angle_a + parent.margin_of_error < diff_angle_b)
                {
                    return -1;
                }
                else if (distance_a <= distance_b)
                {
                    return -1;
                }
            }
            if (diff_angle_a == diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                //Debug.Log("distance a: " + distance_a);
                //Debug.Log("distance b: " + distance_b);

                if (distance_a <= distance_b)
                {
                    return -1;
                }
            }
            if (diff_angle_a > diff_angle_b)
            {
                float distance_a = Vector3.Magnitude(to_a);
                float distance_b = Vector3.Magnitude(to_b);

                //Debug.Log("distance a: " + distance_a);
                //Debug.Log("distance b: " + distance_b);

                if (diff_angle_a > diff_angle_b + parent.margin_of_error)
                {
                    return 1;
                }
                else if (distance_a <= distance_b)
                {
                    return -1;
                }
            }

            return 1;
        }
    }
}