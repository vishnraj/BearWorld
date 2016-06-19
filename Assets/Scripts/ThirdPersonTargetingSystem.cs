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

    public Vector3 direction;
    public GameObject target;
    public GameObject HUD;
    public GameObject Enemies;
    public Camera main_camera;

    float max_distance = 50.0f;

    GameObject targeting_icon;
    Transform targeting_status;
    Transform crosshair;
    CloseCompare compare_distances;
    SortedList<GameObject, string> forward_targets; // enemies sorted by closest forward distance to current target
    SortedList<GameObject, string> backward_targets; // enemies sorted by closest backward distance to current target
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
        forward_targets = new SortedList<GameObject, string>(compare_distances);
        backward_targets = new SortedList<GameObject, string>(compare_distances);

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
                UpdateTargets();
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
            if (locked_on && Input.GetAxis("RightJoystickX") != 0 && Input.GetAxis("RightJoystickY") == 0 && !switching)
            {
                GameObject new_target_forward = null;
                GameObject new_target_back = null;

                if (Input.GetAxis("RightJoystickX") < 0)
                {
                    new_target_forward = FindClosest(ref forward_targets, "L");
                    new_target_back = FindClosest(ref backward_targets, "L");
                }
                else
                {
                    new_target_forward = FindClosest(ref forward_targets, "R");
                    new_target_back = FindClosest(ref backward_targets, "R");
                }

                if (new_target_forward != null && new_target_back != null)
                {
                    if (compare_distances.Compare(new_target_forward, new_target_back) == 1)
                    {
                        to_target = new_target_forward.transform.position - transform.position;
                        if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                        {
                            target = new_target_forward;
                            switching = true;
                        }
                    }
                    else
                    {
                        to_target = new_target_back.transform.position - transform.position;
                        if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                        {
                            target = new_target_back;
                            switching = true;
                        }
                    }
                }
                else if (new_target_forward != null)
                {
                    // modularize
                    to_target = new_target_forward.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                    {
                        target = new_target_forward;
                        switching = true;
                    }
                }
                else if (new_target_back != null)
                {
                    to_target = new_target_back.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                    {
                        target = new_target_back;
                        switching = true;
                    }
                }
            }
            else if (locked_on && Input.GetAxis("RightJoystickX") != 0 && Input.GetAxis("RightJoystickY") != 0 && !switching)
            {
                GameObject new_target = null;

                if (Input.GetAxis("RightJoystickY") < 0)
                {
                    if (Input.GetAxis("RightJoystickX") < 0)
                    {
                        new_target = FindClosest(ref forward_targets, "L");
                    }
                    else
                    {
                        new_target = FindClosest(ref forward_targets, "R");
                    }
                }
                else
                {
                    if (Input.GetAxis("RightJoystickX") < 0)
                    {
                        new_target = FindClosest(ref backward_targets, "L");
                    }
                    else
                    {
                        new_target = FindClosest(ref backward_targets, "R");
                    }
                }

                if (new_target != null)
                {
                    to_target = new_target.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, to_target, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                    {
                        target = new_target;
                        switching = true;
                    }
                }
            }

            if (Input.GetAxis("RightJoystickX") == 0 && Input.GetAxis("RightJoystickY") == 0 && switching)
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

    void UpdateTargets()
    {
        forward_targets.Clear();
        backward_targets.Clear();

        if (target != null)
        {
            Vector3 facing_target = transform.forward;
            Transform target_transform = target.transform;

            Vector3 facing_taget_xz = new Vector3(facing_target.x, 0, facing_target.z);
            float to_target_angle_from_z_axis = rt.CalculateXZRotation(facing_taget_xz);

            int count = 0;
            int forward_count = 0;
            int backward_count = 0;
            for (int i = 0; i < Enemies.transform.childCount; ++i)
            {
                Transform current_enemy = Enemies.transform.GetChild(i);
                Vector3 player_to_enemy = current_enemy.transform.position - transform.position;

                // to make sure we are not looking at the same enemy
                RaycastHit hit;
                if ( current_enemy.GetInstanceID() != target_transform.GetInstanceID() &&
                    Physics.Raycast(transform.position, player_to_enemy, out hit, max_distance, 1 << LayerMask.NameToLayer("Enemy_Layer")))
                {
                    Vector3 target_to_current_enemy = current_enemy.transform.position - target_transform.position;

                    string direction = "";

                    // rotate vector from current target to new enemy towards the z-axis by the same offset of player to target vector
                    // and use this vector to determine if the new target is to the left or right relative to player to target vector
                    Vector3 target_rotated_to_current_enemy_rotated = Quaternion.AngleAxis(-to_target_angle_from_z_axis, Vector3.up) * target_to_current_enemy;
                    if ( target_rotated_to_current_enemy_rotated.x < 0 )
                    {
                        direction = "L";
                    }
                    else
                    {
                        direction = "R";
                    }

                    // select whether the current target is behind or in front
                    Vector3 target_to_current_enemy_xz = new Vector3(target_to_current_enemy.x, 0, target_to_current_enemy.z);
                    float angle_to_new_target = Vector3.Angle(facing_taget_xz, target_to_current_enemy_xz);
                    if (angle_to_new_target < 90)
                    {
                        forward_targets.Add( current_enemy.gameObject, direction ); // the target is in front of the current target
                                                                                   // relative to player's position
                        ++forward_count;
                    }
                    else
                    {
                        backward_targets.Add( current_enemy.gameObject, direction ); // the target is behind the target
                                                                                   // relative to player's position
                        ++backward_count;
                    }

                    ++count;
                }
            }

            Debug.Log("Valid enemies: " + count);
            Debug.Log("forward_count: " + forward_count);
            Debug.Log("backward_count:" + backward_count);
            Debug.Log("Size forward: " + forward_targets.Count);
            Debug.Log("Size backward: " + backward_targets.Count);
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

            float distance_a = Vector3.Magnitude(to_a);
            float distance_b = Vector3.Magnitude(to_b);

            if (distance_a < distance_b)
            {
                return 1;
            }

            return 0;
        }
    }

    GameObject FindClosest(ref SortedList<GameObject, string> desired_map, string direction)
    {
        GameObject desired_obj = null;
        foreach (KeyValuePair<GameObject, string> pair in desired_map)
        {
            if (pair.Value == direction)
            {
                return pair.Key;
            }
        }

        return desired_obj;
    }
}