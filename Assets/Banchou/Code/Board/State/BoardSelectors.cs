using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    public static class BoardSelectors {
        public static IObservable<BoardState> ObserveBoard(this GameState state) {
            return state.Board.Observe();
        }

        public static IObservable<GameState> OnBoardChanged(this GameState state) {
            return state.Board.Observe().Select(_ => state);
        }

        public static IObservable<IReadOnlyReactiveCollection<string>> ObserveLoadingScenes(this GameState state) {
            return state.ObserveBoard()
                .Select(board => board.LoadingScenes);
        }

        public static IObservable<PawnState> ObserveAddedPawns(this GameState state) {
            return state.GetPawns()
                .ObserveAdd()
                .Select(pair => pair.Value);
        }

        public static IObservable<PawnState> ObserveRemovedPawns(this GameState state) {
            return state.GetPawns()
                .ObserveRemove()
                .Select(pair => pair.Value);
        }

        public static IReadOnlyReactiveDictionary<int, PawnState> GetPawns(this GameState state) {
            return state.Board.Pawns;
        }

        public static IEnumerable<int> GetPawnIds(this GameState state) {
            return state.GetPawns().Select(p => p.Key);
        }
    }
}