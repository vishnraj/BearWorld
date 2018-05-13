using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Utility;
using InputEvents;

namespace TargetingEvents {
    public enum TARGETING_EVENT{ INIT, FREE, CAN_LOCK, LOCK_ON };

    public class TargetingPublisher {
        public delegate void TargetingEventHandler(object sender, TARGETING_EVENT e);
        public event TargetingEventHandler TargetingEvent;

        public void OnTargetingEvent(TARGETING_EVENT e) {
            TargetingEventHandler i = TargetingEvent;
            if (i != null) {
                i(this, e);
            } else {
                Debug.Log("NOOP");
            }
        }
    }
}

public class ThirdPersonTargetingSystem : MonoBehaviour
{
    public float current_weapon_range;
    public bool locked_on = false;
    public bool can_lock_on = false;
    public bool switching = false;
    public bool forward_facing = true;

    public Vector3 direction;
    public GameObject target = null;
    public GameObject Enemies;
    public GameObject event_manager;
    public SortedList sorted_by_chosen_direction; // targets sorted by closest distance to current target
                                                  // as well as closeness of the angle of the vector to new target
                                                  // to the angle created by the joystick's coordinates
                                                  // if there is a target, otherwise just sorts using InstanceIDs
                                                  // in ascending order
    public TargetingEvents.TargetingPublisher publisher;

    bool start = false;
    float input_x = 0.0f;
    float input_y = 0.0f;
    float current_joystick_angle = 0.0f;
    float margin_of_error = 5.0f;

    Vector3 previous_target_pos;

    CloseCompare compare_distances;
    Rotation rt;
    BasicCharacter c;

    public delegate void DoUpdate();
    DoUpdate update;

    // Use this for initialization
    void Start()
    {
        previous_target_pos = new Vector3();

        compare_distances = new CloseCompare(this);
        sorted_by_chosen_direction = new SortedList(compare_distances);

        rt = new Rotation();
        c = GetComponent<BasicCharacter>();

        event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;

        update = DefaultUpdate;
    }

    private void Awake() {
        publisher = new TargetingEvents.TargetingPublisher();
    }

    // Update is called once per frame
    void Update()
    {
        if (!start) {
            publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.INIT);
            start = true;
        }

        update();
    }

    void PausedUpdate() {
        if (target == null) {
            DisableLockedOn();
            can_lock_on = false;

            publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.FREE);

            c.SetTarget(direction); // this may not actually be needed
        } else {
            Vector3 to_target = target.transform.position - transform.position;

            if (locked_on && !(to_target.magnitude <= current_weapon_range)) {
                DisableLockedOn();
                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.FREE);
            } else if (!locked_on && to_target.magnitude <= current_weapon_range) {
                can_lock_on = true;
                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.CAN_LOCK);
            }
        }
    }

    void DefaultUpdate() {
        if (target != null) {
            // This all needs to be handled via  state class
            // because this has become unruly garbage

            if (can_lock_on && Input.GetAxis("LeftTriggerAxis") > 0 && !locked_on) {
                locked_on = true;

                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.LOCK_ON);

            } else if (Input.GetAxis("LeftTriggerAxis") == 0 && locked_on) {
                locked_on = false;

                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.CAN_LOCK);
            }

            Vector3 to_target = target.transform.position - transform.position;

            if (locked_on && !(to_target.magnitude <= current_weapon_range)) {
                DisableLockedOn();
                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.FREE);
            } else if (!locked_on && to_target.magnitude <= current_weapon_range) {
                can_lock_on = true;
                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.CAN_LOCK);
            }

            // user input related
            if (locked_on && (Input.GetAxis("RightJoystickX") != 0 || Input.GetAxis("RightJoystickY") != 0) && !switching) {
                switching = true;
                SetNewTarget();
            }

            if (locked_on && (Input.GetAxis("RightJoystickX") == 0 && Input.GetAxis("RightJoystickY") == 0) && switching) {
                switching = false;
            }

            // This is stupid, but for some reason unity, after confirming that in this frame
            // this object exists, seems to show that this is null, even though state of all objects shouldn't get updated until
            // the next loop, which means we can't be here to begin with - I now believe this has more to do with the fact that
            // Unity will run the updates in random order, so it's not guaranteed that we will always be able to rely on state
            // that is set within a different script -> we need to force order within each script
            if (target != null) {
                c.SetTarget(target.transform.position);
                previous_target_pos = target.transform.position;
            }
        } else {
            // This type of behavior needs to be divided
            // up into states for when locked on vs not locked on
            if (locked_on) {
                List<GameObject> enemies = Enemies.GetComponent<EnemyTracker>().GetAllEnemies();
                target = GetClosestEnemy(enemies);

                if (target != null) {
                    Vector3 to_target = target.transform.position - transform.position;
                    if (to_target.magnitude <= current_weapon_range) {
                        c.SetTarget(target.transform.position); // now aiming system will update based on this
                        return;
                    } else {
                        DisableLockedOn();
                    }
                } else {
                    DisableLockedOn();
                }
            }

            if (can_lock_on) {
                publisher.OnTargetingEvent(TargetingEvents.TARGETING_EVENT.FREE);
            }

            can_lock_on = false;
            c.SetTarget(direction);
        }
    }

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    update = PausedUpdate; // this is actually required
                                           // we need to update GUI elements (directly for now)
                                           // if we unequip an item and thus lose the target
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    update = DefaultUpdate;
                }
                break;
            default:
                break;
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

			// Vector3 player_forward = new Vector3(transform.forward.x, 0, transform.forward.z);

			// Vector3 to_new_target = new_target.transform.position - target.transform.position;
			// float angle_to_new_target = rt.CalculateZXRotation(new Vector3(to_new_target.x, 0, to_new_target.z), player_forward);
			//Debug.Log("angle_to_new_target: " + angle_to_new_target);

			// Cos is calculated on y because z is effectively y
			// in our coordinate system
			// new_target_x_unit = Mathf.Sin(Mathf.Deg2Rad * angle_to_new_target);
			// new_target_y_unit = Mathf.Cos(Mathf.Deg2Rad * angle_to_new_target);

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