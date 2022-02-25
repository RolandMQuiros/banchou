using System;
using System.Collections.Generic;
using System.Linq;
using Banchou.Board;
using Banchou.Player;

namespace Banchou.Pawn {
    public static class PawnSelectors {
        public static PawnState GetPawn(this GameState state, int pawnId) {
            state.Board.Pawns.TryGetValue(pawnId, out var pawn);
            return pawn;
        }

        public static PawnSpatial GetPawnSpatial(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.Spatial;
        }

        public static int GetPawnPlayerId(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PlayerId ?? 0;
        }

        public static PlayerState GetPawnPlayer(this GameState state, int pawnId) {
            return state.GetPlayer(state.GetPawnPlayerId(pawnId));
        }

        public static IEnumerable<PawnSpatial> GetPawnSpatials(this GameState state) {
            return state.GetPawns()
                .Values
                .Select(pawn => pawn.Spatial)
                .Where(spatial => spatial != null);
        }

        public static Func<float> PawnTimeScale(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);
            return () => state.Board.TimeScale * (pawn?.TimeScale ?? 1f);
        }

        public static GetDeltaTime PawnDeltaTime(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);
            return () => state.Board.TimeScale * (pawn?.TimeScale ?? 1f) * state.GetDeltaTime();
        }
    }
}