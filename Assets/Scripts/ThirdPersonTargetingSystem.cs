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
    public GameObject target;
    public GameObject HUD;
    public GameObject Enemies;
    public Camera main_camera;
    public SortedList sorted_targets; // targets sorted by closest distance to current target
                                      // as well as closeness of the angle of the vector to new target
                                      // to the angle created by the joystick's coordinates
                                      // if there is a target, otherwise just sorts using InstanceIDs
                                      // in ascending order
    float input_x = 0.0f;
    float input_y = 0.0f;
    float current_joystick_angle = 0.0f;
    float margin_of_error = 10.0f;
    float max_distance = 70.0f;

    GameObject targeting_icon;
    Transform targeting_status;
    Transform crosshair;
    CloseCompare compare_distances;
    Rotation rt;

    // Use this for initialization
    void Start()
    {
        targeting_status = HUD.transform.FindChild("TargetingStatus");
        crosshair = HUD.transform.FindChild("Crosshair");
        targeting_icon = new GameObject();
        targeting_icon.AddComponent<Image>();
        targeting_icon.GetComponent<Image>().sprite = HUD.transform.FindChild("Crosshair").GetComponent<AimingSystem>().target_reticle;
        targeting_icon.name = "TargetingIcon";

        float desired_scale = .4f;

        targeting_icon.GetComponent<Image>().transform.localScale = new Vector3(desired_scale, desired_scale, desired_scale);

        compare_distances = new CloseCompare(this);
        sorted_targets = new SortedList(compare_distances);

        rt = new Rotation();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (target != null)
        {
            // capture target related
            Vector3 to_target = target.transform.position - transform.position;
            if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
            {
                if (target != null && !locked_on)
                {
                    can_lock_on = true;
                    UpdateTargetingStatus();
                }
                else if (!locked_on)
                {
                    can_lock_on = false;
                    UpdateTargetingStatus();
                }
            }
            else if (!locked_on)
            {
                can_lock_on = false;
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

            if (locked_on && to_target.magnitude <= max_distance)
            {
                SetTargetOnGui();
            }
            else
            {
                can_lock_on = false;
                locked_on = false;
                crosshair.GetComponent<AimingSystem>().enabled = true;
                targeting_icon.GetComponent<Image>().enabled = false;
                targeting_icon.transform.SetParent(null);
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
        }
        else
        {
            can_lock_on = false;
            switching = false;

            if (locked_on)
            {
                locked_on = false;
                crosshair.GetComponent<AimingSystem>().enabled = true;
                targeting_icon.GetComponent<Image>().enabled = false;
                targeting_icon.transform.SetParent(null);
            }

            UpdateTargetingStatus();
        }
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
            targeting_icon.GetComponent<Image>().sprite = HUD.transform.FindChild("Crosshair").GetComponent<AimingSystem>().target_reticle;
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

        if ((input_x <= 0 && input_y < 0) || (input_x > 0 && input_y < 0)) 
        {
            current_joystick_angle = rt.CalculateXZRotation(new Vector3(input_x, 0, input_y), -Vector3.forward);
            Debug.Log("here");
        }
        else
            current_joystick_angle = rt.CalculateXZRotation(new Vector3(input_x, 0, input_y));

        if (transform.forward.z < 0)
            forward_facing = false;
        else
            forward_facing = true;

        Debug.Log("Joystick angle: " + current_joystick_angle);

        sorted_targets.Clear();
        for (int i = 0; i < Enemies.transform.childCount; ++i) {
            GameObject current_enemy = Enemies.transform.GetChild(i).gameObject;
            Vector3 ret = main_camera.WorldToViewportPoint(current_enemy.transform.position);

            if (current_enemy.GetInstanceID() != target.GetInstanceID() &&
                (ret.x > 0 && ret.x < 1) && (ret.y > 0 && ret.y < 1)) 
            {
                sorted_targets.Add(current_enemy, current_enemy.GetInstanceID());
            }
        }

        //Debug.Log("Sorted targets count: " + sorted_targets.Count);

        try {
            Debug.Log("Selected Target ID: " + sorted_targets.GetByIndex(0));

            new_target = (GameObject)sorted_targets.GetKey(0);    
            target = new_target;
        } catch (System.ArgumentOutOfRangeException) {
            Debug.Log("All objects are out of range.");
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

            if ((parent.input_x <= 0 && parent.input_y < 0) || (parent.input_x > 0 && parent.input_y < 0)) 
            {
                player_forward = -player_forward;
            }

            float angle_a = parent.rt.CalculateXZRotation(new Vector3(to_a.x, 0, to_a.z), player_forward, parent.forward_facing);
            float angle_b = parent.rt.CalculateXZRotation(new Vector3(to_b.x, 0, to_b.z), player_forward, parent.forward_facing);

            if (parent.current_joystick_angle < 0 && angle_b > 0)
                return -1;
            else if (parent.current_joystick_angle > 0 && angle_b < 0)
                return -1;

            Debug.Log("Instance ID a: " + a.gameObject.GetInstanceID());
            Debug.Log("Instance ID b: " + b.gameObject.GetInstanceID());
            Debug.Log("angle a: " + angle_a);
            Debug.Log("angle b: " + angle_b);

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