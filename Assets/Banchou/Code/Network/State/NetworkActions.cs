namespace Banchou.Network {
    public static class NetworkActions {
        public static GameState StartHost(
            this GameState state,
            int port,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            state.Network.StartHost(port, tickRate, simulateMinLatency, simulateMaxLatency);
            return state;
        }
        
        public static GameState StartHost(
            this GameState state,
            string roomName,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            state.Network.StartHost(roomName, tickRate, simulateMinLatency, simulateMaxLatency);
            return state;
        }

        public static GameState ConnectToServer(
            this GameState state,
            string ip,
            int port,
            string roomName,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            state.Network.ConnectToHost(ip, port, roomName, simulateMinLatency, simulateMaxLatency);
            return state;
        }

        public static GameState SetNetworkStats(this GameState state, int ping, float when) {
            state.Network.Stats.Update(ping, when);
            return state;
        }
    }
}