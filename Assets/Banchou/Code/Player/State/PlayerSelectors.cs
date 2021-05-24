using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

using Banchou.Network;

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
                .Where(player => player.NetworkId == state.GetNetworkId())
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

        public static IReadOnlyDictionary<int, PlayerState> GetPlayers(this GameState state) {
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

        public static InputCommand DirectionToStick(this Vector2 vec) {
            var snapped = Snapping.Snap(vec, Vector2.one);
            if      (snapped == Vector2.up          )  return InputCommand.Forward;
            else if (snapped == Vector2.one         )  return InputCommand.ForwardRight;
            else if (snapped == Vector2.right       )  return InputCommand.Right;
            else if (snapped == new Vector2(1f, -1f))  return InputCommand.BackRight;
            else if (snapped == Vector2.down        )  return InputCommand.Back;
            else if (snapped == -Vector2.one        )  return InputCommand.BackLeft;
            else if (snapped == Vector2.left        )  return InputCommand.Left;
            else if (snapped == new Vector2(-1f, 1f))  return InputCommand.ForwardLeft;
            return InputCommand.None;
        }
    }
}