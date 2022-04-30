using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    public static class BoardSelectors {
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