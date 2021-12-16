using System.Collections.Generic;
using System.Linq;
using Banchou.Board;
using Banchou.Pawn;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantState GetCombatant(this GameState state, int pawnId) => BoardSelectors.GetPawn(state, pawnId)?.Combatant;
        
        public static IEnumerable<CombatantState> GetCombatants(this GameState state) =>
            state.GetPawns().Values
                .Select(pawn => pawn.Combatant)
                .Where(combatant => combatant != null);

        public static IEnumerable<PawnSpatial> GetCombatantSpatials(this GameState state) =>
            state.GetPawns().Values
                .Where(pawn => pawn.Combatant != null && pawn.Spatial != null)
                .Select(pawn => pawn.Spatial);
        
        public static bool IsCombatant(this GameState state, int pawnId) => state.GetCombatant(pawnId) != null;
        public static HitState GetCombatantLastHit(this GameState state, int pawnId) =>
            state.GetCombatant(pawnId)?.LastHit;
        public static AttackState GetCombatantAttack(this GameState state, int pawnId) =>
            state.GetCombatant(pawnId)?.Attack;
        public static float GetNormalizedStunTime(this GameState state, int pawnId) {
            var lastHit = state.GetCombatantLastHit(pawnId);
            return (state.GetTime() - lastHit.LastUpdated) / lastHit.StunTime;
        }
        public static CombatantTeam GetCombatantTeam(this GameState state, int pawnId) =>
            state.GetCombatant(pawnId)?.Stats.Team ?? CombatantTeam.Neutral;
        public static bool AreHostile(this GameState state, int firstPawnId, int secondPawnId) =>
            state.GetCombatantTeam(firstPawnId) != state.GetCombatantTeam(secondPawnId);
    }
}