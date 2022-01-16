using Banchou.Network;

namespace Banchou.Player {
    public static class PlayerActions {
        public static GameState AddPlayer(
            this GameState state,
            out PlayerState player,
            int playerId = default,
            string prefabKey = default
        ) {
            state.Players.AddPlayer(out player, playerId, prefabKey, state.GetNetworkId());
            return state;
        }
        
        public static GameState AddPlayer(
            this GameState state,
            int playerId = default,
            string prefabKey = default
        ) {
            state.Players.AddPlayer(out _, playerId, prefabKey, state.GetNetworkId());
            return state;
        }

        public static GameState RemovePlayer(this GameState state, int playerId) {
            state.Players.RemovePlayer(playerId);
            return state;
        }

        public static GameState SyncInput(this GameState state, PlayerInputState input) {
            state.GetPlayer(input.PlayerId)?.Input?.Sync(input);
            return state;
        }
    }
}