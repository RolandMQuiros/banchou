using Banchou.Board;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantState GetCombatant(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.Combatant;
        }
    }
}