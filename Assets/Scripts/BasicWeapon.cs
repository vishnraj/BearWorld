using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon {
    public abstract class WeaponInfo { }
    public enum WEAPON_TYPE { RANGE, MELEE }

    class WeaponFactory {
        // (kind of a) Factory function
        public GameObject SpawnEquipped(string desired_equipped, Transform character, string body_part = "") {
            GameObject equipped = null;
            switch (desired_equipped) {
                case "Raygun":
                case "Sword": {
                    if (body_part == "") {
                        Debug.LogError("We require a body part to equip weapon of type: " + desired_equipped);
                    }

                    Transform part = character.Find(body_part);
                    if (part != null) {
                        equipped = Object.Instantiate(Resources.Load("Prefabs/" + desired_equipped), part.position, part.rotation) as GameObject;
                        equipped.transform.parent = part;
                    } else {
                        Debug.LogError("No valid body part found for equipping: " + desired_equipped);
                    }
                }
                break;
                case "Bombs": {
                    if (body_part != "") {
                        Transform part = character.Find(body_part);
                        equipped = Object.Instantiate(Resources.Load("Prefabs/" + desired_equipped), part.position, part.rotation) as GameObject;
                        equipped.transform.parent = part;
                    } else {
                        equipped = Object.Instantiate(Resources.Load("Prefabs/" + desired_equipped), character.position, character.rotation) as GameObject;
                        equipped.transform.parent = character;
                    }

                    foreach (Transform child in equipped.transform) {
                        Object.Destroy(child.gameObject);
                    }
                }
                break;
                default: {
                    Debug.LogError("For spawning equipped, no case found.");
                    return new GameObject();
                }
            }

            if (equipped != null) {
                // Some processing to get rid of clone or similar
                // words that show up in the name
                if (equipped.name.Contains(" ")) {
                    equipped.name = equipped.name.Substring(0, equipped.name.LastIndexOf(" "));
                } else if (equipped.name.Contains("(")) {
                    equipped.name = equipped.name.Substring(0, equipped.name.LastIndexOf("("));
                }

                equipped.GetComponent<BasicWeapon>().Init();
            }

            return equipped;
        }
    }
}

public abstract class BasicWeapon : MonoBehaviour {
    public float range;

    protected Weapon.WEAPON_TYPE type;
    protected BasicCharacter c;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void Attack() { }
    public virtual void EndAttack() { }

    public virtual void Init() {
        if (transform.parent == null) {
            Debug.Log("Error, weapon not assigned to parent.");
            return;
        }
    }

    public Weapon.WEAPON_TYPE GetWeaponType() { return type; }
    public void SetCharacter(BasicCharacter character) { c = character; }
}
