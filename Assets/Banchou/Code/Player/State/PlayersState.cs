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

        [field: SerializeField]
        public Dictionary<int, PlayerState> Members { get; init; } = Members ?? new Dictionary<int, PlayerState>();

        public PlayersState AddPlayer(
            out PlayerState player,
            int playerId = default,
            string prefabKey = null,
            int networkId = default
        ) {
            if (playerId == default) {
                // Find the first available pawn ID
                var usedIds = Members.Keys.OrderBy(id => id).ToList();
                for (playerId = 1; playerId <= usedIds.Count && playerId == usedIds[playerId - 1]; playerId++) { }
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
            var playerIds = Members.Select(p => p.Key).ToList();
            var syncPlayerIds = sync.Members.Select(p => p.Key).ToList();

            foreach (var added in syncPlayerIds.Except(playerIds)) {
                Members[added] = sync.Members[added];
            }

            foreach (var removed in playerIds.Except(syncPlayerIds)) {
                Members.Remove(removed);
            }

            foreach (var playerId in playerIds.Intersect(syncPlayerIds)) {
                Members[playerId].Sync(sync.Members[playerId]);
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