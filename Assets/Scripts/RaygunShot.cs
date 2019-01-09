using UnityEngine;
using System.Collections;

public class RaygunShot : DamageDealer {
    public float shot_speed;
    public float interval_before_destruction;
    public float start_distance_before_destruction;
    public float damage;

    bool fired = false;
    bool expired = false;
    float start_time_of_destruction = 0.0f;

    GameObject m_stage;

    Vector3 final_location;
    Vector3 direction;
    Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        m_stage = GameObject.FindGameObjectWithTag("Stage");
        transform.SetParent(m_stage.transform);

        rb = GetComponent<Rigidbody>();
    }

    private void Awake() {
        damager_name = Weapon.DamagerNames.LASER_SHOT;
    }

    // Update is called once per frame
    void Update()
    {
        if (expired && Time.time - start_time_of_destruction >= interval_before_destruction) {
            Destroy(gameObject);
        }
        else if (fired && !expired && Vector3.Magnitude(final_location - transform.position) <= start_distance_before_destruction)
        {
            expired = true;
            start_time_of_destruction = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (fired)
        {
            rb.velocity = direction * shot_speed;
        }
    }

    void OnTriggerEnter(Collider collide) {
        if (collide.tag != "Ammunition" && collide.tag != origin_tag && collide.tag != "Section") {
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

            BasicHealth h = collide.gameObject.GetComponent<BasicHealth>();
            if (h != null) {
                h.Damage(damage);
            }

            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector3 _direction, float range) {
        direction = (_direction - transform.position).normalized;
        // should be the range distance in front of the transform that
        // fired the object in the direction that the shot was fired
        final_location = direction * range + transform.position;
        fired = true;
    }

    public override void Init() {
        base.Init();

        damager_name = Weapon.DamagerNames.LASER_SHOT;
    }
}
