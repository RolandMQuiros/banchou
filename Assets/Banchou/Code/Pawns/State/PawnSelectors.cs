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
    }
}