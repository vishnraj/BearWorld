﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetingEvents;
using PlayerAttackEvents;
using MovementEvents;

public class PlayerAttackController : MonoBehaviour {
    public GameObject event_manager;

    BasicWeapon weapon = null;
    Player character;
    bool attacking = false;

    // used for special attacks
    bool in_special_attack = false;
    bool end_special_attack_step = false;
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
        static HashSet<string> weapon_has_special_attack = null;

        public static HashSet<string> Instance() {
            if (weapon_has_special_attack == null) {
                weapon_has_special_attack = new HashSet<string>();

                weapon_has_special_attack.Add(Weapon.WeaponNames.SWORD);
            }

            return weapon_has_special_attack;
        }
    }

    PlayerAttackControllerPublisher publisher;

    delegate void DoUpdate();
    DoUpdate update;

    void TargetingEventsCallback(GameObject target, TARGETING_EVENT e) {
        switch (e) {
            case TARGETING_EVENT.LOCK_ON: {
                    if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName())) {
                        if (!in_special_attack && attacking) {
                            weapon.EndAttack();
                            attacking = false;
                        }
                    }

                    update = WeaponLockOnUpdate;
                }
                break;
            case TARGETING_EVENT.CAN_LOCK:
            case TARGETING_EVENT.FREE: {
                    // can happen during an unequip
                    if (weapon != null) {
                        if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName()) && in_special_attack) {
                            TerminateSpecialAttack();
                        }
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
                    if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName())) {
                        EndSpecialAttack();
                    }
                }
                break;
            case MOVEMENT_EVENT.SPECIAL_ATTACK_COMPLETE: {
                    if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName())) {
                        attacking = false;
                        in_special_attack = false;
                    }
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
            collision.gameObject.GetComponent<BasicWeapon>() == null && 
            collision.gameObject.GetComponent<DamageDealer>() == null && 
            collision.gameObject != gameObject &&
            collision.gameObject != character.GetTarget() &&
            !collision.gameObject.transform.IsChildOf(character.GetTarget().transform))
        {
            TerminateSpecialAttack();
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (in_special_attack && collision.gameObject.tag != "Ground" &&
            collision.gameObject.GetComponent<BasicWeapon>() == null &&
            collision.gameObject.GetComponent<DamageDealer>() == null &&
            collision.gameObject != gameObject &&
            collision.gameObject != character.GetTarget() &&
            !collision.gameObject.transform.IsChildOf(character.GetTarget().transform)) 
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
                if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName())) {
                    attacking = true;
                    in_special_attack = true;

                    publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_START);
                } else {
                    weapon.Attack();
                    attacking = true;
                }
            }
            else if (Input.GetAxis("RightTriggerAxis") == 0 && attacking) {
                if (!WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName())) {
                    weapon.EndAttack();
                    attacking = false;
                }
            }

            if (end_special_attack_step && WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName()) && 
                (Time.frameCount - attack_frame_end) >= SpecialAttackFramesThreshold.Instance()[weapon.GetWeaponName()]) {
                weapon.EndAttack();
                end_special_attack_step = false;
                publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_COMPLETE);
            }
        } else {
            Debug.Log("Exodia, it's not possible...");
        }
    }

    void TerminateSpecialAttack() {
        if (end_special_attack_step) {
            weapon.EndAttack();
        }

        in_special_attack = false;
        end_special_attack_step = false;
        attacking = false;
        publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_TERMINATE);
    }

    void EndSpecialAttack() {
        if (attacking) {
            weapon.Attack();
        }
        
        end_special_attack_step = true;
        publisher.OnPlayerAttackEvent(weapon.GetWeaponName(), PLAYER_ATTACK_EVENT.SPECIAL_ATTACK_END);
        attack_frame_end = Time.frameCount;
    }

    // Public Functions used by other scripts, when they need to set something in this
    // script directly (mainly for scripts that are very closely related to this and will
    // actually make changes to state in the same frame)

    public bool IsAttacking() { return attacking;  }

    public void EquipWeapon(BasicWeapon w, Player c) {
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
            if (WeaponHasSpecialAttack.Instance().Contains(weapon.GetWeaponName()) && in_special_attack) {
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
