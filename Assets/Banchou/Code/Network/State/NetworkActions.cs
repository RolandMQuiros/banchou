namespace Banchou.Network {
    public static class NetworkActions {
        public static GameState StartServer(
            this GameState state,
            int port,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            state.Network.StartServer(port, tickRate, simulateMinLatency, simulateMaxLatency);
            return state;
        }

        public static GameState ConnectToServer(
            this GameState state,
            string ip,
            int port,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            state.Network.ConnectToServer(ip, port, simulateMinLatency, simulateMaxLatency);
            return state;
        }

        public static GameState SetNetworkStats(this GameState state, int ping, float when) {
            state.Network.Stats.Update(ping, when);
            return state;
        }
    }
}