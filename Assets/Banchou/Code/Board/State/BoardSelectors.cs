using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    public static class BoardSelectors {
        public static IObservable<BoardState> ObserveBoard(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Board.Observe());
        }

        public static IObservable<GameState> OnBoardChanged(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Board.Observe().Select(_ => state));
        }

        public static IObservable<IEnumerable<string>> ObserveLoadingScenes(this IObservable<GameState> observeState) {
            return observeState
                .ObserveBoard()
                .Select(board => board.LoadingScenes);
        }

        public static IObservable<IEnumerable<int>> ObservePawnIds(this IObservable<GameState> observeState) {
            return observeState.OnBoardChanged()
                .Select(state => state.GetPawnIds());
        }

        public static IReadOnlyDictionary<int, PawnState> GetPawns(this GameState state) {
            return state.Board.Pawns;
        }

        public static IEnumerable<int> GetPawnIds(this GameState state) {
            return state.GetPawns().Keys;
        }
    }
}