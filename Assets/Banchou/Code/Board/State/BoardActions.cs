using Photon.Pun;
using UnityEngine;

using Banchou.Network;

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

        public static GameState AddPawn(this GameState state, int pawnId, string prefabKey, int playerId, Vector3 position) {
            switch (state.GetNetworkMode()) {
                case NetworkMode.Host:
                case NetworkMode.Local:
                    state.Board.AddPawn(
                        pawnId,
                        prefabKey,
                        networkId: PhotonNetwork.AllocateViewID(true),
                        playerId,
                        position,
                        Vector3.forward,
                        state.GetTime()
                    );
                    break;
            }
            return state;
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