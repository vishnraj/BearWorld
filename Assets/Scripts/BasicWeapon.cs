using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon {
    public enum WEAPON_TYPE { RANGE, MELEE }

    static class WeaponNames {
        public const string SWORD = "Sword";

        public const string RAYGUN = "Raygun";
        public const string BOMBS = "Bombs";
    }

    static class DamagerNames {
        public const string BLADE = "Blade";

        public const string LASER_SHOT = "RaygunLaserShot";
        public const string BOMB = "Bomb";
    }

    static class WeaponTypeMap {
        static Dictionary<string, WEAPON_TYPE> weapon_name_to_type = null;

        public static Dictionary<string, WEAPON_TYPE> Instance() {
            if (weapon_name_to_type == null) {
                weapon_name_to_type = new Dictionary<string, WEAPON_TYPE>();

                weapon_name_to_type[WeaponNames.SWORD] = WEAPON_TYPE.MELEE;

                weapon_name_to_type[WeaponNames.RAYGUN] = WEAPON_TYPE.RANGE;
                weapon_name_to_type[WeaponNames.BOMBS] = WEAPON_TYPE.RANGE;
            }

            return weapon_name_to_type;
        }
    }

    static class PlayerSpawnOnBody {
        public const string DEFAULT_SPAWN = "RightArm";
    }

    class WeaponFactory {
        // (kind of a) Factory function
        public GameObject SpawnEquipped(string desired_equipped, Transform character, string body_part = "") {
            GameObject equipped = null;
            switch (desired_equipped) {
                case WeaponNames.RAYGUN :
                case WeaponNames.SWORD : {
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
                case WeaponNames.BOMBS : {
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
                    return equipped;
                }
            }

            if (equipped != null) {
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
    protected string weapon_name = "";

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

        // this is important - make sure that the weapon itself
        // is never used during a hitscan
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public Weapon.WEAPON_TYPE GetWeaponType() { return type; }
    public string GetWeaponName() { return weapon_name; }

    public void SetCharacter(BasicCharacter character) { c = character; }
}
