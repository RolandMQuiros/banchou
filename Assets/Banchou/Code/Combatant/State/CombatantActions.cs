using UnityEngine;
using Banchou.Pawn;

namespace Banchou.Combatant {
    public static class CombatantActions {
        public static CombatantState GetCombatant(this GameState state, int pawnId) {
            CombatantState combatant;
            state.Board.Combatants.Members.TryGetValue(pawnId, out combatant);
            return combatant;
        }

        public static CombatantState HitCombatant(
            this GameState state,
            int targetPawnId,
            int strength,
            Vector3 direction,
            Vector3 knockBack
        ) {
            var target = state.GetCombatant(targetPawnId);
            var targetSpatial = state.GetPawnSpatial(targetPawnId);
            if (target != null && targetSpatial != null) {
                var planarDirection = Vector3.ProjectOnPlane(direction, targetSpatial.Up);
                var incomingDirection = new Vector2(
                    Vector3.Dot(direction, targetSpatial.Forward),
                    Vector3.Dot(direction, targetSpatial.Right)
                ).DirectionToBlock();
                return target.Hit(strength, incomingDirection, knockBack, state.GetTime());
            }
            return null;
        }

    }
}