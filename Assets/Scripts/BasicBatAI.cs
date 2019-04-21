using System.Collections;
using UnityEngine;
using Utility;
using InputEvents;

public class BasicBatAI : BasicEnemyAI {
    public float speed;
    public float circular_speed;
    public float attack_interval;
    public float attack_distance;

    IEnumerator engage;
    IEnumerator end_step;
    bool attacking = false;
    bool in_circular = false;
    float angle = 0;

    Rotation r;

    GameObject m_event_manager;

    void GlobalInputEventsCallback(object sender, INPUT_EVENT e) {
        switch (e) {
            case INPUT_EVENT.PAUSE: {
                    enabled = false;
                }
                break;
            case INPUT_EVENT.UNPAUSE: {
                    enabled = true;
                }
                break;
            default:
                break;
        }
    }

    private void OnDestroy() {
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent -= GlobalInputEventsCallback;
    }

    // Use this for initialization
    void Start() {
        Init();

        m_event_manager = GameObject.Find("GlobalEventManager");
        m_event_manager.GetComponent<InputManager>().publisher.InputEvent += GlobalInputEventsCallback;
    }

    IEnumerator FinishDrop() {
        yield return new WaitForSeconds(attack_interval);
        w.EndAttack();
        attacking = false;

        end_step = null;
        yield return new WaitForSeconds(0.0f);
    }

    // Update is called once per frame
    void Update() {
        // travel towards target, without changing the y direction

        if (target != null) {
            c.SetTarget(target);
            c.SetAimingDirection(target.transform.position);

            // need to preserve height
            Vector3 xz_target_pos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            Vector3 to_target = xz_target_pos - transform.position;

            if (!attacking) {
                w.Attack();

                if (end_step == null) {
                    end_step = FinishDrop();
                    attacking = true;
                    StartCoroutine(end_step);
                }
            }

            Vector3 direction = Vector3.RotateTowards(transform.forward, to_target, Mathf.PI, 0);
            transform.rotation = Quaternion.LookRotation(direction);

            if (to_target.magnitude > attack_distance) {
                in_circular = false;

                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, xz_target_pos, step);
            } else {
                // circular motion

                // Find where we are in the path
                if (!in_circular) {
                    Vector3 normalized_loc = -(to_target.normalized);
                    angle = r.CalculateZXRotation(new Vector3(normalized_loc.x, 0, normalized_loc.z));
                    angle = ((2 * Mathf.PI) / 360) * angle;

                    in_circular = true;
                }

                angle += circular_speed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * to_target.magnitude;
                transform.position = xz_target_pos + offset;
            }
        } else {
            w.EndAttack();
        }
    }

    protected override void Init() {
        base.Init();
        r = new Rotation();
    }
}
