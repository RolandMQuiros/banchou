using UnityEngine;
using Banchou.Pawn;

namespace Banchou.Board {
    public static class BoardActions {
        public static GameState LoadScene(this GameState state, string sceneName) {
            state.Board.LoadScene(sceneName);
            return state;
        }

        public static GameState UnloadScene(this GameState state, string sceneName) {
            state.Board.UnloadScene(sceneName);
            return state;
        }

        public static GameState AddPawn(
            this GameState state,
            out PawnState pawn,
            int pawnId = default,
            string prefabKey = default,
            int playerId = default,
            Vector3 position = default,
            Vector3 forward = default
        ) {
            state.Board.AddPawn(
                state.GetTime(),
                out pawn,
                pawnId,
                prefabKey,
                playerId,
                position,
                forward
            );
            return state;
        }

        public static GameState AddPawn(
            this GameState state,
            int pawnId = default,
            string prefabKey = default,
            int playerId = default,
            Vector3 position = default,
            Vector3 forward = default
        ) {
            return state.AddPawn(out _, pawnId, prefabKey, playerId, position, forward);
        }

        public static GameState RemovePawn(this GameState state, int pawnId) {
            state.Board.RemovePawn(pawnId, state.GetTime());
            return state;
        }

        public static GameState ClearPawns(this GameState state) {
            state.Board.ClearPawns(state.GetTime());
            return state;
        }
    }
}