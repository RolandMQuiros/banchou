using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

using Banchou.Network;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IReadOnlyDictionary<int, PlayerState> GetPlayers(this GameState state) {
            return state.Players.Members;
        }

        public static PlayerState GetPlayer(this GameState state, int playerId) {
            PlayerState player = null;
            state.GetPlayers().TryGetValue(playerId, out player);
            return player;
        }

        public static bool IsLocalPlayer(this GameState state, int playerId) {
            return state.GetPlayer(playerId).NetworkId == state.GetNetworkId();
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