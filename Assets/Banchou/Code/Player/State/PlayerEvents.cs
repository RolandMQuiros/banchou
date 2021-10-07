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
                .SelectMany(players => {
                    PlayerState player;
                    if (players.Members.TryGetValue(playerId, out player)) {
                        return player.Observe();
                    }
                    return Observable.Empty<PlayerState>();
                });
        }

        public static IObservable<PlayerInputState> ObservePlayerInput(this GameState state, int playerId) {
            return state
                .ObservePlayer(playerId)
                .SelectMany(player => player.Input.Observe());
        }

        public static IObservable<PlayerInputState> ObserveLocalPlayerInput(this GameState state) {
            return state
                .ObservePlayers()
                .SelectMany(players => players.Members.Values)
                .SelectMany(player => player.Observe())
                .Where(player => state.IsLocalPlayer(player.PlayerId))
                .SelectMany(player => player.Input.Observe());
        }

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