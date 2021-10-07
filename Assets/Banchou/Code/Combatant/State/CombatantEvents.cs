using System;
using Banchou.Pawn;
using UniRx;

namespace Banchou.Combatant {
    public static class CombatantEvents {
        public static IObservable<CombatantState> ObserveCombatant(this GameState state, int pawnId) {
            var observable = state.ObservePawn(pawnId)
                .SelectMany(pawn => pawn?.Combatant.Observe());
            var combatant = state.GetCombatant(pawnId);
            if (combatant != null) {
                observable = observable.StartWith(combatant);
            }
            return observable;
        }
    }
}