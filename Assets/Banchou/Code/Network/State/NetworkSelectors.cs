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

        public static NetworkMode GetNetworkMode(this GameState state) {
            return state.Network.Mode;
        }

        public static RollbackState GetRollback(this GameState state) {
            return state.Network.Rollback;
        }

        public static RollbackPhase GetRollbackPhase(this GameState state) {
            return state.GetRollback().Phase;
        }

        public static float GetCorrectionTime(this GameState state) {
            return state.GetRollback().CorrectionTime;
        }

        public static float GetCorrectionDelta(this GameState state) {
            return state.GetRollback().DeltaTime;
        }

        public static IPEndPoint GetServerEndpoint(this GameState state) {
            var ip = IPAddress.Parse(state.Network.ServerIP);
            return new IPEndPoint(ip, state.Network.ServerPort);
        }

        public static bool IsSimulatingLatency(this GameState state) {
            return state.Network.SimulateMinLatency > 0  && state.Network.SimulateMaxLatency > 0;
        }

        public static (int Min, int Max) GetSimulatedLatency(this GameState state) {
            return (Min: state.Network.SimulateMinLatency, Max: state.Network.SimulateMaxLatency);
        }
    }
}