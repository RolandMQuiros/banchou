using System;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IObservable<PlayersState> ObservePlayers(this GameState state) {
            return Observable.FromEvent<PlayersState>(
                h => state.Players.Changed += h,
                h => state.Players.Changed -= h
            );
        }

        public static IDictionary<int, PlayerState> GetPlayers(this GameState state) {
            return state.Players.Members;
        }

        public static PlayerState GetPlayer(this GameState state, int playerId) {
            PlayerState player = null;
            state.GetPlayers().TryGetValue(playerId, out player);
            return player;
        }

        public static IObservable<PlayerState> ObservePlayer(this GameState state, int playerId) {
            var player = state.GetPlayer(playerId);
            if (player == null) {
                return Observable.Empty<PlayerState>();
            }
            return Observable.FromEvent<PlayerState>(
                h => player.Changed += h,
                h => player.Changed -= h
            ).StartWith(player);
        }

        public static IEnumerable<int> GetPlayerIds(this GameState state) {
            return state.Players.Members.Keys;
        }

        public static string GetPlayerPrefabKey(this GameState state, int playerId) {
            return state.GetPlayer(playerId)?.PrefabKey;
        }
    }
}