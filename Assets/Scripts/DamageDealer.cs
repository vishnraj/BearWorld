﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageDealer : MonoBehaviour {
    protected string origin_tag = "";
    protected string damager_name = "";

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual float DealDamage(float health, int id) { return health; /* do nothing by default */ }

    public void SetOriginTag(string t) {
        origin_tag = t;
    }

    public string GetOriginTag() {
        return origin_tag;
    }

    public string GetDamagerName() { return damager_name; }

    public virtual void Init() { /* do nothing by default */ }
}
