using Banchou.Pawn;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantActions {
        public static GameState SetCombatant(
            this GameState state,
            out CombatantState combatant,
            CombatantTeam team,
            int pawnId,
            int maxHealth
        ) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                Debug.LogError($"No Pawn {pawnId} found for combatant");
                combatant = null;
            } else {
                pawn.SetCombatant(team, maxHealth, state.GetTime(), out combatant);
            }
            return state;
        }

        public static GameState SetCombatant(this GameState state, CombatantTeam team, int pawnId, int maxHealth) {
            return state.SetCombatant(out _, team, pawnId, maxHealth);
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
            float hitStun,
            int damage,
            bool isGrab,
            bool lockOffOnConfirm
        ) {
            var attacker = state.GetPawn(attackerPawnId);
            var defender = state.GetPawn(defenderPawnId);

            if (attacker == null) {
                Debug.LogError($"No Pawn {attackerPawnId} found for combatant");
            }

            if (defender == null) {
                Debug.LogError($"No Pawn {defenderPawnId} found for combatant");
            }

            if (attacker?.Combatant != null && defender?.Combatant?.Defense?.IsInvincible == false) {
                blocked &= !isGrab; // Grabs break through blocks
                var now = state.GetTime();
                
                if (isGrab) {
                    attacker.Combatant.Grab.Contact(attackId, defenderPawnId, now);
                }

                if (!isGrab || attacker.Combatant.Grab.TargetId == defenderPawnId) {
                    attacker.Combatant.Attack.Connect(
                        defenderPawnId,
                        damage,
                        blocked,
                        hitPause,
                        hitStun,
                        contact,
                        knockback,
                        recoil,
                        isGrab,
                        now
                    );
                }

                if (lockOffOnConfirm) {
                    attacker.Combatant.LockOff(now);
                }

                defender.Combatant.Hit(damage, now);
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