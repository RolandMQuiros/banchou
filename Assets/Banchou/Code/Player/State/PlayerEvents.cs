using System;
using UniRx;

namespace Banchou.Player {
    public static class PlayerEvents {
        public static IObservable<PlayersState> ObservePlayers(this GameState state) {
            return state.Players.Observe();
        }

        public static IObservable<PlayerState> ObservePlayer(this GameState state, int playerId) {
            return state
                .ObservePlayers()
                .Select(players => {
                    players.Members.TryGetValue(playerId, out var player);
                    return player;
                })
                // .DefaultIfEmpty(state.GetPlayer(playerId))
                .Where(player => player != null);
        }

        public static IObservable<PlayerState> ObservePlayerChanges(this GameState state, int playerId) {
            return state.ObservePlayer(playerId).SelectMany(player => player.Observe());
        }

        public static IObservable<PlayerInputState> ObservePlayerInputChanges(this GameState state, int playerId) {
            return state
                .ObservePlayer(playerId)
                .SelectMany(player => player.Input.Observe());
        }

        public static IObservable<PlayerInputState> ObserveLocalPlayerInputChanges(this GameState state) {
            return state
                .ObservePlayers()
                .SelectMany(players => players.Members.Values)
                .Where(player => state.IsLocalPlayer(player.PlayerId))
                .SelectMany(player => player.Input.Observe());
        }

        public static IObservable<PlayerInputState> ObserveAllPlayerInputChanges(this GameState state) =>
            state.ObservePlayers()
                .SelectMany(players => players.Members.Values)
                .SelectMany(player => player.Input.Observe());

        public static IObservable<PlayerState> ObserveAddedPlayers(this GameState state) {
            return Observable
                .FromEvent<PlayerState>(
                    h => state.Players.PlayerAdded += h,
                    h => state.Players.PlayerAdded -= h
                )
                .StartWith(state.Players.Members.Values);
        }

        public static IObservable<PlayerState> ObserveRemovedPlayers(this GameState state) {
            return Observable
                .FromEvent<PlayerState>(
                    h => state.Players.PlayerRemoved += h,
                    h => state.Players.PlayerRemoved -= h
                );
        }
    }
}