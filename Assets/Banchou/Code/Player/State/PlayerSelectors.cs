using System;
using System.Collections.Generic;
using UniRx;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IObservable<Unit> ObservePlayers(this GameState state) {
            return Observable.FromEvent(
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

        public static IEnumerable<int> GetPlayerIds(this GameState state) {
            return state.Players.Members.Keys;
        }

        public static IList<InputUnit> GetPlayerInputs(this GameState state, int playerId) {
            return state.GetPlayer(playerId)?.Inputs;
        }
    }
}