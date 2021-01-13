using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Board {
    public static class BoardSelectors {
        public static IObservable<BoardState> ObserveBoard(this GameState state) {
            return Observable.FromEvent<BoardState>(
                h => state.Board.Changed += h,
                h => state.Board.Changed -= h
            );
        }

        public static IEnumerable<int> GetPawnIds(this GameState state) {
            return state.Board.Pawns.Keys;
        }
    }
}