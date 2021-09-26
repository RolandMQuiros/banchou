using System;
using Banchou.Board;
using UniRx;

namespace Banchou.Combatant {
    public static class CombatantEvents {
        public static IObservable<CombatantState> ObserveCombatant(this GameState state, int pawnId) {
            var observable = state.ObserveBoard()
                .SelectMany(board => board.Combatants.Observe())
                .SelectMany(combatantsState => {
                    if (combatantsState.Members.TryGetValue(pawnId, out var combatant)) {
                        return combatant.Observe();
                    }
                    return Observable.Empty<CombatantState>();
                });
            
            var combatant = state.GetCombatant(pawnId);
            if (combatant != null) {
                observable = observable.StartWith(combatant);
            }

            return observable;
        }
    }
}