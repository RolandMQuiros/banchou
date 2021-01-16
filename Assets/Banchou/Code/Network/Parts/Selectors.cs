using System;
using UniRx;

namespace Banchou.Network {
    public static class NetworkSelectors {
        public static IObservable<NetworkState> ObserveNetwork(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Network.Observe());
        }

        public static IObservable<GameState> OnNetworkChanged(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Network.Observe().Select(_ => state));
        }

        public static IObservable<RollbackState> ObserveRollback(this IObservable<GameState> observeState) {
            return observeState
                .ObserveNetwork()
                .SelectMany(network => network.Rollback.Observe());
        }

        public static RollbackState GetRollback(this GameState state) {
            return state.Network.Rollback;
        }
    }
}