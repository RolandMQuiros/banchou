using System.Linq;
using System.Collections.Generic;
using Banchou.Board;
using UnityEngine;

using Banchou.Network;
using Banchou.Pawn;

namespace Banchou.Player {
    public static class PlayersSelectors {
        public static IReadOnlyDictionary<int, PlayerState> GetPlayers(this GameState state) {
            return state.Players.Members;
        }

        public static PlayerState GetPlayer(this GameState state, int playerId) {
            state.GetPlayers().TryGetValue(playerId, out var player);
            return player;
        }

        public static PlayerInputState GetPlayerInput(this GameState state, int playerId) =>
            state.GetPlayer(playerId).Input;

        public static IEnumerable<PawnState> GetPlayerPawns(this GameState state, int playerId) {
            return state.GetPawns().Values
                .Where(pawn => pawn.PlayerId == playerId);
        }

        public static IEnumerable<PawnSpatial> GetPlayerPawnSpatials(this GameState state, int playerId) {
            return state.GetPlayerPawns(playerId)
                .Select(pawn => pawn.Spatial);
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
            if (vec.magnitude < 0.8f) return InputCommand.Neutral;
            var snapped = Snapping.Snap(vec, Vector2.one);
            if (snapped == Vector2.up          )  return InputCommand.Forward;
            if (snapped == Vector2.one         )  return InputCommand.ForwardRight;
            if (snapped == Vector2.right       )  return InputCommand.Right;
            if (snapped == new Vector2(1f, -1f))  return InputCommand.BackRight;
            if (snapped == Vector2.down        )  return InputCommand.Back;
            if (snapped == -Vector2.one        )  return InputCommand.BackLeft;
            if (snapped == Vector2.left        )  return InputCommand.Left;
            if (snapped == new Vector2(-1f, 1f))  return InputCommand.ForwardLeft;
            return InputCommand.None;
        }
    }
}