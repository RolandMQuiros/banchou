using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    public static class BoardSelectors {
        public static IObservable<BoardState> ObserveBoardChanges(this GameState state) {
            return state.Board.Observe();
        }

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

        public static IReadOnlyDictionary<int, PawnState> GetPawns(this GameState state) {
            return state.Board.Pawns;
        }

        public static IEnumerable<PawnSpatial> GetPawnSpatials(this GameState state) {
            return state.GetPawns()
                .Values
                .Select(pawn => pawn.Spatial)
                .Where(spatial => spatial != null);
        }

        public static IEnumerable<int> GetPawnIds(this GameState state) {
            return state.GetPawns().Select(p => p.Key);
        }

        public static bool AreScenesLoading(this GameState state) {
            return state.Board.LoadingScenes.Any();
        }

        public static bool IsSceneLoaded(this GameState state, string sceneName) {
            return state.Board.ActiveScenes.Contains(sceneName);
        }
    }
}