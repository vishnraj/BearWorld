using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BasicCharacter {
    bool in_lock_on = false;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetInLockOn(bool locked_on) { in_lock_on = locked_on; }
    public bool InLockOn() { return in_lock_on; }
}
