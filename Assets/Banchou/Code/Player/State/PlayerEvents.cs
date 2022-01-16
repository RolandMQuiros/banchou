using System;
using System.Collections.Generic;
using System.Linq;
using Banchou.Board;
using Banchou.Pawn;
using UniRx;

namespace Banchou.Player {
    public static class PlayerEvents {
        public static IObservable<PlayersState> ObservePlayers(this GameState state) => state.Players.Observe();

        public static IObservable<PlayerState> ObservePlayer(this GameState state, int playerId) =>
            state.ObservePlayers()
                .Select(players => {
                    players.Members.TryGetValue(playerId, out var player);
                    return player;
                })
                .Where(player => player != null)
                .DistinctUntilChanged();

        public static IObservable<PlayerState> ObservePlayerChanges(this GameState state, int playerId) =>
            state.ObservePlayer(playerId)
                .SelectMany(player => player.Observe());

        public static IObservable<IEnumerable<PawnState>> ObservePlayerPawns(this GameState state, int playerId) =>
            state.ObserveBoardChanges()
                .SelectMany(
                    board => board.Pawns.Values
                        .Select(pawn => pawn.Observe().DistinctUntilChanged(p => p.PlayerId))
                        .Select(_ => board)
                )
                .Select(
                    board => board.Pawns.Values
                        .Where(pawn => pawn.PlayerId == playerId)
                );

        public static IObservable<PlayerInputState> ObservePlayerInputChanges(this GameState state, int playerId) =>
            state.ObservePlayer(playerId)
                .SelectMany(player => player.Input.Observe());

        public static IObservable<PlayerInputState> ObserveLocalPlayerInputChanges(this GameState state) =>
            state.ObservePlayers()
                .SelectMany(players => players.Members.Values)
                .Where(player => state.IsLocalPlayer(player.PlayerId))
                .SelectMany(player => player.Input.Observe());

        public static IObservable<PlayerInputState> ObserveAllPlayerInputChanges(this GameState state) =>
            state.ObservePlayers()
                .SelectMany(players => players.Members.Values)
                .SelectMany(player => player.Input.Observe());

        public static IObservable<PlayerState> ObserveAddedPlayers(this GameState state) =>
            Observable.FromEvent<PlayerState>(
                    h => state.Players.PlayerAdded += h,
                    h => state.Players.PlayerAdded -= h
                )
                .StartWith(state.Players.Members.Values);

        public static IObservable<PlayerState> ObserveRemovedPlayers(this GameState state) =>
            Observable.FromEvent<PlayerState>(
                    h => state.Players.PlayerRemoved += h,
                    h => state.Players.PlayerRemoved -= h
                );
    }
}