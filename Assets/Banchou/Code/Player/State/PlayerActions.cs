namespace Banchou.Player {
    public static class PlayerActions {
        public static GameState AddPlayer(this GameState state, int playerId, string prefabKey, int networkId = 0) {
            state.Players.AddPlayer(playerId, prefabKey, networkId);
            return state;
        }

        public static GameState RemovePlayer(this GameState state, int playerId) {
            state.Players.RemovePlayer(playerId);
            return state;
        }
    }
}