using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    [MessagePackObject, Serializable]
    public record PlayersState(Dictionary<int, PlayerState> Members = null) : Notifiable<PlayersState> {
        public event Action<PlayerState> PlayerAdded;
        public event Action<PlayerState> PlayerRemoved;

        [Key(0)][field: SerializeField]
        public Dictionary<int, PlayerState> Members { get; init; } = Members ?? new Dictionary<int, PlayerState>();

        public override void Dispose() {
            base.Dispose();
            PlayerAdded = null;
            PlayerRemoved = null;
            foreach (var player in Members.Values) {
                player.Dispose();
            }
        }

        public PlayersState AddPlayer(
            out PlayerState player,
            int playerId = default,
            string prefabKey = null,
            int networkId = default
        ) {
            if (playerId == default) {
                playerId = (Members.Values.Count(p => p.PrefabKey == prefabKey), prefabKey).GetHashCode();
            }

            player = new PlayerState(playerId, prefabKey, networkId);
            Members[playerId] = player;
            PlayerAdded?.Invoke(player);

            return Notify();
        }

        public PlayersState RemovePlayer(int playerId) {
            PlayerState player;
            if (Members.TryGetValue(playerId, out player) && Members.Remove(playerId)) {
                PlayerRemoved?.Invoke(player);
                return Notify();
            }
            return this;
        }

        public PlayersState ClearPlayers(float when) {
            if (Members.Any()) {
                if (PlayerRemoved != null) {
                    foreach (var pawn in Members.Values) {
                        PlayerRemoved(pawn);
                    }
                }
                Members.Clear();
                return Notify();
            }
            return this;
        }

        public PlayersState SyncGame(PlayersState sync) {
            var currentPlayers = Members.Values.Select(p => (p.PlayerId, p.PrefabKey)).ToList();
            var incomingPlayers = sync.Members.Values.Select(p => (p.PlayerId, p.PrefabKey)).ToList();

            foreach (var removed in currentPlayers.Except(incomingPlayers)) {
                var player = Members[removed.PlayerId];
                Members.Remove(removed.PlayerId);
                PlayerRemoved?.Invoke(player);
            }

            foreach (var added in incomingPlayers.Except(currentPlayers)) {
                var player = sync.Members[added.PlayerId];
                Members[player.PlayerId] = player;
                PlayerAdded?.Invoke(player);
            }

            foreach (var updated in currentPlayers.Intersect(incomingPlayers)) {
                Members[updated.PlayerId].Sync(sync.Members[updated.PlayerId]);
            }

            return Notify();
        }

        public PlayersState SyncBoard(IEnumerable<PlayerState> incoming) {
            foreach (var incomingPlayer in incoming) {
                PlayerState player;
                if (Members.TryGetValue(incomingPlayer.PlayerId, out player)) {
                    player.Sync(incomingPlayer);
                }
            }
            return this;
        }
    }
}