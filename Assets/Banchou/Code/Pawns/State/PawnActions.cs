using UnityEngine;

namespace Banchou.Pawn {
    public static class PawnActions {
        public static GameState AttachPlayerToPawn(this GameState state, int pawnId, int playerId) {
            state.GetPawn(pawnId)?.AttachPlayer(playerId, state.GetTime());
            return state;
        }

        public static GameState AttachAnimatorToPawn(this GameState state, int pawnId, out PawnAnimatorFrame frame) {
            frame = null;
            state.GetPawn(pawnId)?.AttachAnimator(state.GetTime(), out frame);
            return state;
        }

        public static GameState MovePawn(this GameState state, int pawnId, Vector3 velocity) {
            state.GetPawnSpatial(pawnId)?.Move(velocity, state.GetTime());
            return state;
        }

        public static GameState SyncSpatial(this GameState state, PawnSpatial sync) {
            state.GetPawn(sync.PawnId)?.Spatial?.Sync(sync);
            return state;
        }
    }
}