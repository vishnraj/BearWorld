using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerHealthEvents;

public class RealmUpdate : MonoBehaviour {
    [SerializeField]
    GameObject m_sections;
    [SerializeField]
    GameObject m_stage;
    [SerializeField]
    GameObject m_player;

    [SerializeField]
    GameObject m_event_manager;

    void PlayerHealthEventsCallback(float health, PLAYER_HEALTH_EVENT e) {
        switch (e) {
            case PLAYER_HEALTH_EVENT.DEAD: {
                    enabled = false;
                }
                break;
            default:
                break;
        }
    }

    // Use this for initialization
    void Start () {
        m_event_manager.GetComponent<ComponentEventManager>().health_publisher.PlayerHealthEvent += PlayerHealthEventsCallback;
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
