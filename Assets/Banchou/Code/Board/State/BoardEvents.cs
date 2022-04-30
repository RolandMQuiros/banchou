using System;
using Banchou.Pawn;
using UniRx;

namespace Banchou.Board {
    public static class BoardEvents {
        public static IObservable<BoardState> ObserveBoardChanges(this GameState state) => state.Board.Observe();

        public static IObservable<string> ObserveAddedScenes(this GameState state) {
            return Observable
                .FromEvent<string>(
                    h => state.Board.SceneAdded += h,
                    h => state.Board.SceneAdded -= h
                )
                .StartWith(state.Board.LoadingScenes);
        }

        public static IObservable<string> ObserveRemovedScenes(this GameState state) {
            return Observable
                .FromEvent<string>(
                    h => state.Board.SceneRemoved += h,
                    h => state.Board.SceneRemoved -= h
                );
        }

        public static IObservable<PawnState> ObserveAddedPawns(this GameState state) {
            return Observable
                .FromEvent<PawnState>(
                    h => state.Board.PawnAdded += h,
                    h => state.Board.PawnAdded -= h
                )
                .StartWith(state.GetPawns().Values);
        }

        public static IObservable<PawnState> ObserveRemovedPawns(this GameState state) {
            return Observable
                .FromEvent<PawnState>(
                    h => state.Board.PawnRemoved += h,
                    h => state.Board.PawnRemoved -= h
                );
        }
    }
}