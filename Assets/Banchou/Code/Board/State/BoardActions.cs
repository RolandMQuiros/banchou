using UnityEngine;

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

        public static GameState AddPawn(this GameState state, int pawnId, string prefabKey, int playerId, Vector3 position, float when) {
            state.Board.AddPawn(pawnId, prefabKey, playerId, position, Vector3.forward, when);
            return state;
        }

        public static GameState RemovePawn(this GameState state, int pawnId, float when) {
            state.Board.RemovePawn(pawnId, when);
            return state;
        }

        public static GameState ClearPawns(this GameState state, float when) {
            state.Board.ClearPawns(when);
            return state;
        }
    }
}