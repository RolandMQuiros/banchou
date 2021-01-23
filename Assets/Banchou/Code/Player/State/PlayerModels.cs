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
        [Key(3)] public PlayerInputStates Input { get; private set; } = new PlayerInputStates();

        #region Serialization constructors
        public PlayerState() { }
        public PlayerState(int playerId, string prefabKey, int networkId, PlayerInputStates input) {
            PlayerId = playerId;
            PrefabKey = prefabKey;
            NetworkId = networkId;
            Input = input;
        }
        #endregion

        public PlayerState(
            int playerId,
            string prefabKey,
            int networkId = 0
        ) {
            PlayerId = playerId;
            PrefabKey = prefabKey;
            NetworkId = networkId;
        }

        public PlayerState Sync(PlayerState other) {
            Input.Sync(other.Input);
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public class PlayerInputStates : Notifiable<PlayerInputStates> {
        [Key(0)][field: SerializeField] public InputCommand Commands { get; private set; }
        [Key(1)][field: SerializeField] public Vector3 Direction { get; private set; }

        // Look input is not shared across the network
        [IgnoreMember] public Vector2 Look { get; private set; }
        [Key(2)][field: SerializeField] public long Sequence { get; private set; }
        [Key(3)][field: SerializeField] public float When { get; private set; }

        #region Serialization constructors
        public PlayerInputStates() { }
        public PlayerInputStates(InputCommand commands, Vector3 direction, Vector2 look, long sequence, float when) {
            Commands = commands;
            Direction = direction;
            Look = look;
            Sequence = sequence;
            When = when;
        }
        #endregion

        public PlayerInputStates PushMove(Vector3 direction, long sequence, float when) {
            Direction = direction;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputStates PushLook(Vector2 look, long sequence, float when) {
            Look = look;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputStates PushCommands(InputCommand commands, long sequence, float when) {
            Commands = commands;
            Sequence = sequence;
            When = when;

            Notify();
            return this;
        }

        public PlayerInputStates Sync(PlayerInputStates sync) {
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

        [Key(0)] public IReadOnlyDictionary<int, PlayerState> Members => _members;
        [Key(0)] private Dictionary<int, PlayerState> _members = new Dictionary<int, PlayerState>();

        #region Serialization constructors
        public PlayersState() { }
        public PlayersState(IReadOnlyDictionary<int, PlayerState> members) {
            _members = members.ToDictionary(p => p.Key, p => p.Value);
        }
        #endregion

        public PlayersState AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            var player = new PlayerState(
                playerId: playerId,
                prefabKey: prefabKey,
                networkId: networkId
            );
            _members[playerId] = player;
            PlayerAdded?.Invoke(player);
            Notify();
            return this;
        }

        public PlayersState RemovePlayer(int playerId) {
            PlayerState player;
            if (_members.TryGetValue(playerId, out player) && _members.Remove(playerId)) {
                PlayerRemoved?.Invoke(player);
                Notify();
            }
            return this;
        }

        public PlayersState SyncGame(PlayersState sync) {
            var playerIds = _members.Select(p => p.Key);
            var syncPlayerIds = sync.Members.Select(p => p.Key);

            foreach (var added in syncPlayerIds.Except(playerIds)) {
                _members[added] = sync.Members[added];
            }

            foreach (var removed in playerIds.Except(syncPlayerIds)) {
                _members.Remove(removed);
            }

            foreach (var playerId in playerIds.Intersect(syncPlayerIds)) {
                _members[playerId].Sync(sync.Members[playerId]);
            }

            Notify();
            return this;
        }

        public PlayersState SyncBoard(IList<PlayerState> incoming) {
            for (int i = 0; i < incoming.Count; i++) {
                PlayerState player;
                if (Members.TryGetValue(incoming[i].PlayerId, out player)) {
                    player.Sync(incoming[i]);
                }
            }
            return this;
        }
    }

    public delegate int GetPlayerId();
}