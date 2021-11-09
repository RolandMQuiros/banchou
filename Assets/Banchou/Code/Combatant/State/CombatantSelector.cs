using Banchou.Board;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantState GetCombatant(this GameState state, int pawnId) => state.GetPawn(pawnId)?.Combatant;
        public static bool IsCombatant(this GameState state, int pawnId) => state.GetCombatant(pawnId) != null;
        public static HitState GetCombatantLastHit(this GameState state, int pawnId) =>
            state.GetCombatant(pawnId)?.LastHit;

        public static AttackState GetCombatantAttack(this GameState state, int pawnId) =>
            state.GetCombatant(pawnId)?.Attack;
        
        public static float GetNormalizedStunTime(this GameState state, int pawnId) {
            var lastHit = state.GetCombatantLastHit(pawnId);
            return (state.GetTime() - lastHit.LastUpdated) / lastHit.StunTime;
        }
    }
}