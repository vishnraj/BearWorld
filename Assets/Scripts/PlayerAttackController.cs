using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetingEvents;
using PlayerAttackEvents;

public class PlayerAttackController : MonoBehaviour {
    public GameObject event_manager;

    BasicWeapon weapon = null;
    BasicCharacter character;
    bool attacking = false;

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
                    update = DefaultUpdate;
                }
                break;
            default:
                break;
        }
    }


    // Use this for initialization
    void Start () {
        publisher = event_manager.GetComponent<ComponentEventManager>().attacks_publisher;

        event_manager.GetComponent<ComponentEventManager>().targeting_publisher.TargetingEvent += TargetingEventsCallback;
    }

    private void Awake() {
        update = DefaultUpdate;
    }

    // Update is called once per frame
    void Update () {
        update();
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
                weapon.Attack();
                attacking = true;
                publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_START);
            }
            else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
                weapon.EndAttack();
                attacking = false;

                // this may not have any real meaning - even if the trigger is let go, if an attack was started, generally
                // unless we are about to unequip the item, we shouldn't need to send this message out
                // publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END);
            }
        } else {
            Debug.Log("Exodia, it's not possible...");
        }
    }

    public bool IsAttacking() { return attacking;  }

    public void UnsetAttacking() {
        
    }

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
            if (update == WeaponLockOnUpdate) {
                publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END);
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
