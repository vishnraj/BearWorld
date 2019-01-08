using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealmUpdate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // current approach - send a ray cast from the player forward, left, right, up and down
        // for each "barrier" or "ground" tagged object that is hit first by the ray cast, do the following for each object in "stage":
            // limit the objects that are in the current_realm (visible in the camera (outside of enemies) to those that fall in bounds
            // of the barriers and ground objects that were encountered (comparing position for each object to that of the bounding objects)
    }
}
