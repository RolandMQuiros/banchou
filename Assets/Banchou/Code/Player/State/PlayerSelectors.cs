using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IObservable<PlayersState> ObservePlayers(this GameState state) {
            return state.Players.Observe();
        }

        public static IObservable<PlayerState> ObservePlayer(this GameState state, int playerId) {
            return state
                .ObservePlayers()
                .SelectMany(players => {
                    PlayerState player;
                    if (players.Members.TryGetValue(playerId, out player)) {
                        return player.Observe();
                    }
                    return Observable.Empty<PlayerState>();
                });
        }

        public static IObservable<PlayerState> ObserveAddedPlayers(this GameState state) {
            return state.Players.Members
                .ObserveAdd()
                .Select(pair => pair.Value)
                .StartWith(state.Players.Members.Select(pair => pair.Value));
        }

        public static IObservable<PlayerState> ObserveRemovedPlayers(this GameState state) {
            return state.Players.Members
                .ObserveRemove()
                .Select(pair => pair.Value);
        }

        public static IReadOnlyReactiveDictionary<int, PlayerState> GetPlayers(this GameState state) {
            return state.Players.Members;
        }

        public static PlayerState GetPlayer(this GameState state, int playerId) {
            PlayerState player = null;
            state.GetPlayers().TryGetValue(playerId, out player);
            return player;
        }

        public static IEnumerable<int> GetPlayerIds(this GameState state) {
            return state.GetPlayers().Select(p => p.Key);
        }

        public static string GetPlayerPrefabKey(this GameState state, int playerId) {
            return state.GetPlayer(playerId)?.PrefabKey;
        }
    }
}