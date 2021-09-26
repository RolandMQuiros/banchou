using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantState GetCombatant(this GameState state, int pawnId) {
            state.Board.Combatants.Members.TryGetValue(pawnId, out var combatant);
            return combatant;
        }
    }
}