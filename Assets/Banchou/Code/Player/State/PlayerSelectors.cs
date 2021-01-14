using System;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IObservable<PlayersState> ObservePlayers(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Players.Observe());
        }

        public static IObservable<GameState> OnPlayersChanged(this IObservable<GameState> observeState) {
            return observeState
                .SelectMany(state => state.Players.Observe().Select(_ => state));
        }

        public static IObservable<PlayerState> ObservePlayer(this IObservable<GameState> observeState, int playerId) {
            return observeState
                .ObservePlayers()
                .SelectMany(players => {
                    PlayerState player;
                    if (players.Members.TryGetValue(playerId, out player)) {
                        return player.Observe();
                    }
                    return Observable.Empty<PlayerState>();
                });
        }

        public static IObservable<GameState> OnPlayerChanged(this IObservable<GameState> observeState, int playerId) {
            return observeState
                .OnPlayersChanged()
                .SelectMany(state => {
                    var player = state.GetPlayer(playerId);
                    if (player != null) {
                        return player.Observe().Select(_ => state);
                    }
                    return Observable.Empty<GameState>();
                });
        }

        public static IDictionary<int, PlayerState> GetPlayers(this GameState state) {
            return state.Players.Members;
        }

        public static PlayerState GetPlayer(this GameState state, int playerId) {
            PlayerState player = null;
            state.GetPlayers().TryGetValue(playerId, out player);
            return player;
        }

        public static IEnumerable<int> GetPlayerIds(this GameState state) {
            return state.Players.Members.Keys;
        }

        public static string GetPlayerPrefabKey(this GameState state, int playerId) {
            return state.GetPlayer(playerId)?.PrefabKey;
        }

        public static InputUnit GetPlayerInput(this GameState state, int playerId) {
            return state.GetPlayer(playerId)?.LastInput ?? InputUnit.Empty;
        }
    }
}