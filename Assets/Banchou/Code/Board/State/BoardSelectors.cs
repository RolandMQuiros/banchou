using System;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Board {
    public static class BoardSelectors {
        public static IObservable<Unit> ObserveBoard(this GameState state) {
            return Observable.FromEvent(
                h => state.Board.Changed += h,
                h => state.Board.Changed -= h
            );
        }

        public static IEnumerable<int> GetPawnIds(this GameState state) {
            return state.Board.Pawns.Keys;
        }
    }
}