using Banchou.Pawn;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantActions {
        public static GameState SetCombatant(
            this GameState state,
            out CombatantState combatant,
            int pawnId
        ) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                Debug.LogError($"No Pawn {pawnId} found for combatant");
                combatant = null;
            } else {
                pawn.SetCombatant(out combatant, state.GetTime());
            }
            return state;
        }

        public static GameState SetCombatant(this GameState state, int pawnId) {
            return state.SetCombatant(out _, pawnId);
        }

        public static GameState HitCombatant(
            this GameState state,
            Vector3 contact,
            int attackId,
            int attackerPawnId,
            int defenderPawnId,
            bool blocked,
            bool countered,
            Vector3 knockback,
            Vector3 recoil,
            float hitPause,
            float attackPause,
            float hitStun,
            int damage,
            bool isGrab,
            bool lockOffOnConfirm
        ) {
            var attacker = state.GetPawn(attackerPawnId);
            var defender = state.GetPawn(defenderPawnId);
            var now = state.GetTime();

            if (attacker == null) {
                Debug.LogError($"No Pawn {attackerPawnId} found for combatant");
            }

            if (defender == null) {
                Debug.LogError($"No Pawn {defenderPawnId} found for combatant");
            }

            if (attacker?.Combatant != null && defender?.Combatant?.Defense?.IsInvincible == false) {
                var style = HitStyle.Confirmed;
                if (isGrab) {
                    style = HitStyle.Grabbed;
                } else if (blocked) {
                    style = HitStyle.Blocked;
                }

                if (isGrab) {
                    attacker.Combatant.Grab.Contact(attackId, defenderPawnId, now);
                }

                if (!isGrab || attacker.Combatant.Grab.TargetId == defenderPawnId) {
                    var attack = attacker.Combatant.Attack;
                    
                    // Only pause once per attack
                    if (attack.AttackId == attackId && attack.PauseTime > 0f) {
                        attackPause = 0f;
                    }
                    
                    attacker.Combatant.Attack.Connect(
                        defenderPawnId, damage, style, attackPause, contact, knockback, recoil, now
                    );
                }

                if (lockOffOnConfirm) {
                    attacker.Combatant.LockOff(now);
                }

                defender.Combatant.TakeHit(attackerPawnId, attackId, style, damage, hitPause, hitStun, knockback,
                    contact, now);
            }
            return state;
        }

        public static GameState CombatantGuard(this GameState state, int pawnId, float guardTime) {
            var spatial = state.GetPawnSpatial(pawnId);
            if (spatial == null) {
                Debug.LogError($"No Pawn {pawnId} found for combatant");
            } else {
                state.GetCombatant(pawnId)?
                    .Defense
                    .Guard(GuardStyle.Standard, state.GetTime());
            }
            return state;
        }

        public static GameState SetCombatantInvincibility(this GameState state, int pawnId, bool isInvincible) {
            var combatant = state.GetCombatant(pawnId);
            combatant?.Defense.SetInvincibility(isInvincible, state.GetTime());
            return state;
        }
    }
}