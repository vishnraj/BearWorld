using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealmUpdate : MonoBehaviour {
    [SerializeField]
    GameObject m_sections;
    [SerializeField]
    GameObject m_stage;
    [SerializeField]
    GameObject m_player;

	// Use this for initialization
	void Start () {
		
	}

    public static void SetDefaultLayerRecursively(Transform t) {
        t.gameObject.layer = LayerMask.NameToLayer("Default");

        foreach (Transform child in t) {
            SetDefaultLayerRecursively(child);
        }
    }

    public static void SetRenderLayerRecursively(Transform t) {
        if (t.gameObject.GetComponent<EnemyHealth>() != null) {
            t.gameObject.layer = LayerMask.NameToLayer("Enemy_Layer");

        }
        else {
            t.gameObject.layer = LayerMask.NameToLayer("Current_Realm");
        }

        foreach (Transform child in t) {
            SetRenderLayerRecursively(child);
        }
    }

    void LateUpdate () {
        // The player generally is in the current_realm, but new weapons may not be
        SetRenderLayerRecursively(m_player.transform);

        List<Collider> current_relevant_sections = new List<Collider>();
        foreach (Transform child in m_sections.transform) {
            Collider c = child.GetComponent<Collider>();
            if (c.bounds.Contains(m_player.transform.position)) {
                current_relevant_sections.Add(c);
            }
        }

        foreach (Transform child in m_stage.transform) {
            bool found = false;
            foreach (Collider c in current_relevant_sections) {
                if (c.bounds.Contains(child.position)) {
                    found = true;
                    break;
                }
            }

            if (found) {
                SetRenderLayerRecursively(child);               
            } else {
                SetDefaultLayerRecursively(child);
            }
        }
    }
}
