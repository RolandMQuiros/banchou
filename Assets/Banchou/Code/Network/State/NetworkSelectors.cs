using System;
using System.Net;
using UniRx;

namespace Banchou.Network {
    public static class NetworkSelectors {
        public static IObservable<NetworkState> ObserveNetwork(this GameState state) {
            return state.Network.Observe();
        }

        public static IObservable<GameState> OnNetworkChanged(this GameState state) {
            return state.ObserveNetwork().Select(_ => state);
        }

        public static IObservable<int> ObserveTickRate(this GameState state) {
            return state.ObserveNetwork()
                .Select(network => network.TickRate);
        }

        public static int GetNetworkId(this GameState state) => state.Network.NetworkId;
        public static bool IsConnected(this GameState state) => state.GetNetworkId() != 0;
        public static NetworkMode GetNetworkMode(this GameState state) => state.Network.Mode;
        public static RollbackState GetRollback(this GameState state) => state.Network.Rollback;
        public static RollbackPhase GetRollbackPhase(this GameState state) => state.GetRollback().Phase;
        public static float GetCorrectionTime(this GameState state) => state.GetRollback().CorrectionTime;
        public static float GetCorrectionDelta(this GameState state) => state.GetRollback().DeltaTime;
        public static string GetHostName(this GameState state) => state.Network.HostName;
        public static string GetRoomName(this GameState state) => state.Network.RoomName;
        public static IPEndPoint GetHostEndpoint(this GameState state) {
            IPAddress.TryParse(state.Network.HostName, out var ip);
            return new IPEndPoint(ip, state.Network.HostPort);
        }

        public static bool IsSimulatingLatency(this GameState state) {
            return state.Network.SimulateMinLatency > 0  && state.Network.SimulateMaxLatency > 0;
        }

        public static (int Min, int Max) GetSimulatedLatency(this GameState state) {
            return (Min: state.Network.SimulateMinLatency, Max: state.Network.SimulateMaxLatency);
        }
    }
}