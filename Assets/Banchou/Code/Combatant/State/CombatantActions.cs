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

            if (attacker?.Combatant != null && defender?.Combatant != null) {
                attacker.Combatant.Attack.Connect(
                    defenderPawnId,
                    damage,
                    blocked,
                    hitPause,
                    contact,
                    knockback,
                    recoil,
                    state.GetTime()
                );

                if (lockOffOnConfirm) {
                    attacker.Combatant.LockOff(state.GetTime());
                }

                defender.Combatant.Hit(
                    attackerPawnId,
                    attackId,
                    contact,
                    knockback,
                    blocked,
                    hitPause,
                    hitStun,
                    damage,
                    state.GetTime()
                );
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
                    .Guard(GuardStyle.Standard, guardTime, state.GetTime());
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