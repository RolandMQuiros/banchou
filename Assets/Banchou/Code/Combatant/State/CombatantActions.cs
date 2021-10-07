using Banchou.Pawn;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantActions {
        public static GameState SetCombatant(
            this GameState state,
            out CombatantState combatant,
            int pawnId,
            int maxHealth
        ) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                Debug.LogError($"No Pawn {pawnId} found for combatant");
                combatant = null;
            } else {
                pawn.SetCombatant(maxHealth, state.GetTime(), out combatant);
            }
            return state;
        }

        public static GameState HitCombatant(
            this GameState state,
            int attackerPawnId,
            int defenderPawnId,
            Vector3 knockback,
            float hitStun,
            int damage
        ) {
            var attacker = state.GetPawnSpatial(attackerPawnId);
            var defender = state.GetPawnSpatial(defenderPawnId);

            if (attacker == null) {
                Debug.LogError($"No Pawn {attackerPawnId} found for combatant");
            }

            if (defender == null) {
                Debug.LogError($"No Pawn {defenderPawnId} found for combatant");
            }

            if (attacker != null && defender != null) {
                var attackDirection = attacker.Position - defender.Position;
                state.GetCombatant(defenderPawnId)?
                    .Hit(defender.Forward, attackDirection, knockback, hitStun, damage, state.GetTime());
            }
            return state;
        }

        public static GameState CombatantGuard(this GameState state, int pawnId, float guardTime) {
            var spatial = state.GetPawnSpatial(pawnId);
            if (spatial == null) {
                Debug.LogError($"No Pawn {pawnId} found for combatant");
            } else {
                state.GetCombatant(pawnId)?
                    .Guard(guardTime, state.GetTime());
            }
            return state;
        }
    }
}