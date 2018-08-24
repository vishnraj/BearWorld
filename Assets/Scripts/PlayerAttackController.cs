using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetingEvents;
using PlayerAttackEvents;
using MovementEvents;

public class PlayerAttackController : MonoBehaviour {
    public GameObject event_manager;

    BasicWeapon weapon = null;
    BasicCharacter character;
    bool attacking = false;

    // used for special attacks
    bool in_special_attack = false;
    int attack_frame_end = 0;

    static class SpecialAttackFramesThreshold {
        static Dictionary<string, int> weapon_frames_thresholds = null;

        public static Dictionary<string, int> Instance() {
            if (weapon_frames_thresholds == null) {
                weapon_frames_thresholds = new Dictionary<string, int>();

                weapon_frames_thresholds[Weapon.WeaponNames.SWORD] = 10;
            }

            return weapon_frames_thresholds;
        }
    }

    static class WeaponHasSpecialAttack {
        static Dictionary<string, bool> weapon_has_special_attack = null;

        public static Dictionary<string, bool> Instance() {
            if (weapon_has_special_attack == null) {
                weapon_has_special_attack = new Dictionary<string, bool>();

                weapon_has_special_attack[Weapon.WeaponNames.SWORD] = true;

                weapon_has_special_attack[Weapon.WeaponNames.RAYGUN] = false;
                weapon_has_special_attack[Weapon.WeaponNames.BOMBS] = false;
            }

            return weapon_has_special_attack;
        }
    }

    delegate bool AttackVerify();

    static class IsPossibleSpecialAttack {
        static Dictionary<string, AttackVerify> special_attack_verifiers = null;

        public static Dictionary<string, AttackVerify> Instance(PlayerAttackController parent) {
            if (special_attack_verifiers == null) {
                special_attack_verifiers = new Dictionary<string, AttackVerify>();

                special_attack_verifiers[Weapon.WeaponNames.SWORD] = parent.CanSpecialSwordAttack;
            }

            return special_attack_verifiers;
        }
    }

    PlayerAttackControllerPublisher publisher;

    delegate void DoUpdate();
    DoUpdate update;

    void TargetingEventsCallback(GameObject target, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.LOCK_ON: {
                    update = WeaponLockOnUpdate;
                }
                break;
            case TARGETING_EVENT.CAN_LOCK:
            case TARGETING_EVENT.FREE: {
                    if (WeaponHasSpecialAttack.Instance()[weapon.GetWeaponName()] && in_special_attack) {
                        TerminateSpecialAttack();
                    }

                    update = DefaultUpdate;
                }
                break;
            default:
                break;
        }
    }

    void MovementEventCallback(MOVEMENT_EVENT e) {
        switch (e) {
            case MOVEMENT_EVENT.SPECIAL_ATTACK_END: {
                    weapon.Attack();
                    in_special_attack = false;
                    publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END);
                    attack_frame_end = Time.frameCount;
                }
                break;
            default:
                break;
        }
    }

    bool CanSpecialSwordAttack() {
        RaycastHit hit;

        Vector3 target_position = character.GetTarget().transform.position;
        Vector3 to_target = target_position - transform.position;

        int layers = (1 << LayerMask.NameToLayer("Enemy_Layer")) | (1 << LayerMask.NameToLayer("Current_Realm"));

        if (Physics.Raycast(transform.position, to_target, out hit, weapon.range, layers) && hit.collider.gameObject != character.GetTarget()) {
            return false;
        }

        return true;
    }

    // Use this for initialization
    void Start () {
        publisher = event_manager.GetComponent<ComponentEventManager>().attacks_publisher;

        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventsCallback;
        event_manager.GetComponent<ComponentEventManager>().movement_publisher.MovementEvent += MovementEventCallback;
    }

    private void Awake() {
        update = DefaultUpdate;
    }

    // Update is called once per frame
    void Update () {
        update();
    }

    private void OnCollisionEnter(Collision collision) {
        if (in_special_attack && collision.gameObject.tag != "Ground" && 
            (collision.gameObject != gameObject || collision.gameObject != character.GetTarget())) 
        {
            TerminateSpecialAttack();
        }
    }

    void DefaultUpdate() {
        if (weapon != null) {
            if (Input.GetAxis("RightTriggerAxis") > 0 && !attacking) {
                weapon.Attack();
                attacking = true;
            }
            else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
                weapon.EndAttack();
                attacking = false;
            }
        } else {
            Debug.Log("Exodia, it's not possible...");
        }
    }

    void WeaponLockOnUpdate() {
        if (weapon != null) {
            if (Input.GetAxis("RightTriggerAxis") > 0 && !attacking) {
                if (WeaponHasSpecialAttack.Instance()[weapon.GetWeaponName()] && IsPossibleSpecialAttack.Instance(this)[weapon.GetWeaponName()]()) {
                    publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_START);
                    in_special_attack = true;
                } else {
                    weapon.Attack();
                }

                attacking = true;
            }
            else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
                if (!WeaponHasSpecialAttack.Instance()[weapon.GetWeaponName()]) {
                    weapon.EndAttack();
                    attacking = false;
                }
            }

            // Need to wait at least 1 frame for a special attack start and end to be visible
            if (attacking && WeaponHasSpecialAttack.Instance()[weapon.GetWeaponName()] && !in_special_attack && 
                (Time.frameCount - attack_frame_end) > SpecialAttackFramesThreshold.Instance()[weapon.GetWeaponName()]) {
                weapon.EndAttack();
                attacking = false;
            }
        } else {
            Debug.Log("Exodia, it's not possible...");
        }
    }

    void TerminateSpecialAttack() {
        in_special_attack = false;
        attacking = false;
        publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_TERMINATE);
    }

    // Public Functions used by other scripts, when they need to set something in this
    // script directly (mainly for scripts that are very closely related to this and will
    // actually make changes to state in the same frame)

    public bool IsAttacking() { return attacking;  }

    public void EquipWeapon(BasicWeapon w, BasicCharacter c) {
        if (w == weapon) return; // unless we switch, do nothing

        // in this case we are selecting a new weapon, so all bets are off
        // on what we were doing before - reset states - this also basically
        // acts as an unequip
        UnequipWeapon();

        weapon = w;
        character = c;

        weapon.SetCharacter(character);
    }

    public void UnequipWeapon() {
        if (attacking) {
            attacking = false;

            // if something was performing some task based on a special attack for a previous weapon
            // best to notify it to let it know that this is about to change - it can figure out what
            // to do from there, allowing for some continuation in task or just stopping immediately
            if (WeaponHasSpecialAttack.Instance()[weapon.GetWeaponName()] && in_special_attack) {
                TerminateSpecialAttack();
            }
        }

        weapon = null;
    }

    public void EnableController() {
        weapon.enabled = true;
        enabled = true;
    }

    public void DisableController() {
        weapon.enabled = false;
        enabled = false;
    }
}
