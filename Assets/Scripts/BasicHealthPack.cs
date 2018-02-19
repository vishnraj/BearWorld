using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHealthPack : MonoBehaviour {
    public float stored_health;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision) {
        GameObject g = collision.gameObject;
        if (g.tag == "Player") {
            BasicHealth h = g.GetComponent<BasicHealth>();
            h.Heal(stored_health);
            Destroy(gameObject);
        }
    }
}
