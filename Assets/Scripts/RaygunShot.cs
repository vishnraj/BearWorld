using UnityEngine;
using System.Collections;
using Utility;

public class RaygunShot : MonoBehaviour
{
    float shot_speed = 50.0f;
    bool fired = false;
    bool expired = false;
    float start_time_of_destruction = 0.0f;
    float interval_before_destruction = .5f;

    Vector3 final_location;
    Vector3 direction;
    Rigidbody rb;
    Searching s;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        s = new Searching();
    }

    // Update is called once per frame
    void Update()
    {
        if (expired && Time.time - start_time_of_destruction >= interval_before_destruction)
        {
            Destroy(gameObject);
        }
        else if (fired && !expired && Vector3.Magnitude(final_location - gameObject.transform.position) <= 5.0f)
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

    void OnTriggerEnter(Collider collide)
    {

        if (collide.tag == "Enemy")
        {
            GameObject obj = s.FindComponentUpHierarchy<EnemyHealth>(collide.transform);
            if (obj != null)
            {
                // eventually there can be a function in EnemyHealth that takes input of bodypart
                // hashed to the normal amount that is lost for said body part
                // until that body part is destroyed
                obj.GetComponent<EnemyHealth>().health -= 1;
            }
        }

        if (collide.tag != "Weapon" && collide.tag != "Player" && collide.tag != "Ammunition")
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector3 _direction)
    {
        final_location = _direction;
        direction = (_direction - transform.position).normalized;
        fired = true;
    }
}
