using UnityEngine;

namespace Banchou.Pawn {
    public static class PawnActions {
        public static GameState AttachPlayerToPawn(this GameState state, int pawnId, int playerId, float when) {
            state.GetPawn(pawnId)?.AttachPlayer(playerId, when);
            return state;
        }

        public static GameState MovePawn(this GameState state, int pawnId, Vector3 velocity, float when) {
            state.GetPawnSpatial(pawnId)?.Move(velocity, when);
            return state;
        }
    }
}