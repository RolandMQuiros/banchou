using Banchou.Board;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantState GetCombatant(this GameState state, int pawnId) => state.GetPawn(pawnId)?.Combatant;
        public static bool IsCombatant(this GameState state, int pawnId) => state.GetCombatant(pawnId) != null;
    }
}