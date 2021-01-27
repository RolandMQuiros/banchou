using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    [MessagePackObject, Serializable]
    public class PlayerState : Notifiable<PlayerState> {
        [Key(0)] public readonly int PlayerId;
        [Key(1)] public readonly string PrefabKey;
        [Key(2)][field: SerializeField] public int NetworkId { get; private set; }
        [Key(3)][field: SerializeField] public PlayerInputState Input { get; private set; }

        [SerializationConstructor]
        public PlayerState(int playerId, string prefabKey, int networkId, PlayerInputState input) {
            PlayerId = playerId;
            PrefabKey = prefabKey;
            NetworkId = networkId;
            Input = input;
        }

        public PlayerState(
            int playerId,
            string prefabKey,
            int networkId = 0
        ) {
            PlayerId = playerId;
            PrefabKey = prefabKey;
            NetworkId = networkId;
            Input = new PlayerInputState(playerId);
        }

        public PlayerState Sync(PlayerState other) {
            Input.Sync(other.Input);
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public class PlayerInputState : Notifiable<PlayerInputState> {
        [Key(0)] public readonly int PlayerId;
        [Key(1)][field: SerializeField] public InputCommand Commands { get; private set; }
        [Key(2)][field: SerializeField] public Vector3 Direction { get; private set; }

        // Look input is not shared across the network
        [IgnoreMember][field: SerializeField] public Vector2 Look { get; private set; }
        [Key(3)][field: SerializeField] public long Sequence { get; private set; }
        [Key(4)][field: SerializeField] public float When { get; private set; }

        public PlayerInputState(int playerId) {
            PlayerId = playerId;
        }

        [SerializationConstructor]
        public PlayerInputState(int playerId, InputCommand commands, Vector3 direction, long sequence, float when) {
            PlayerId = playerId;
            Commands = commands;
            Direction = direction;
            Sequence = sequence;
            When = when;
        }

        public PlayerInputState Push(InputCommand commands, Vector3 direction, Vector2 look, long sequence, float when) {
            Commands = commands;
            Direction = direction;
            Look = look;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputState PushMove(Vector3 direction, long sequence, float when) {
            Direction = direction;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputState PushLook(Vector2 look, long sequence, float when) {
            Look = look;
            Sequence = sequence;
            When = when;

            // Notify();
            return this;
        }

        public PlayerInputState PushCommands(InputCommand commands, long sequence, float when) {
            Commands = commands;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputState Sync(PlayerInputState sync) {
            Commands = sync.Commands;
            Direction = sync.Direction;
            Look = sync.Look;
            Sequence = sync.Sequence;
            When = sync.When;
            Notify();
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public class PlayersState : Notifiable<PlayersState> {
        public event Action<PlayerState> PlayerAdded;
        public event Action<PlayerState> PlayerRemoved;

        [Key(0)][field: SerializeField] public Dictionary<int, PlayerState> Members { get; private set; } = new Dictionary<int, PlayerState>();

        public PlayersState() { }

        [SerializationConstructor]
        public PlayersState(Dictionary<int, PlayerState> members) {
            Members = members;
        }

        public PlayersState AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            var player = new PlayerState(
                playerId: playerId,
                prefabKey: prefabKey,
                networkId: networkId
            );
            Members[playerId] = player;
            PlayerAdded?.Invoke(player);
            Notify();
            return this;
        }

        public PlayersState RemovePlayer(int playerId) {
            PlayerState player;
            if (Members.TryGetValue(playerId, out player) && Members.Remove(playerId)) {
                PlayerRemoved?.Invoke(player);
                Notify();
            }
            return this;
        }

        public PlayersState SyncGame(PlayersState sync) {
            var playerIds = Members.Select(p => p.Key);
            var syncPlayerIds = sync.Members.Select(p => p.Key);

            foreach (var added in syncPlayerIds.Except(playerIds)) {
                Members[added] = sync.Members[added];
            }

            foreach (var removed in playerIds.Except(syncPlayerIds)) {
                Members.Remove(removed);
            }

            foreach (var playerId in playerIds.Intersect(syncPlayerIds)) {
                Members[playerId].Sync(sync.Members[playerId]);
            }

            Notify();
            return this;
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

    public delegate int GetPlayerId();
}