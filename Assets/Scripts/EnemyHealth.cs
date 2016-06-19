using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float health = 10;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
