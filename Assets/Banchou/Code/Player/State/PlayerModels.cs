using System;
using System.Linq;
using MessagePack;
using UniRx;
using UnityEngine;

namespace Banchou.Player {
    [MessagePackObject, Serializable]
    public class PlayerState : Notifiable<PlayerState> {
        [Key(0)] public readonly int PlayerId;
        [Key(1)] public readonly string PrefabKey;

        [IgnoreMember] public int NetworkId => _networkId;
        [Key(2), SerializeField] private int _networkId;

        [IgnoreMember] public PlayerInputStates Input => _input;
        [Key(3), SerializeField] private PlayerInputStates _input = new PlayerInputStates();

        public PlayerState() { }
        public PlayerState(
            int playerId,
            string prefabKey,
            int networkId = 0
        ) {
            PlayerId = playerId;
            PrefabKey = prefabKey;
            _networkId = networkId;
        }

        public PlayerState SyncGame(PlayerState other) {
            _input.SyncGame(other.Input);
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public class PlayerInputStates : Notifiable<PlayerInputStates> {
        [IgnoreMember] public InputCommand Commands => _commands;
        [Key(0), SerializeField] private InputCommand _commands;

        [IgnoreMember] public Vector3 Direction => _direction;
        [Key(1), SerializeField] private Vector3 _direction;

        // Look input is not shared across the network
        [IgnoreMember] public Vector2 Look => _look;
        [SerializeField] private Vector2 _look;

        [IgnoreMember] public long Sequence => _sequence;
        [Key(2), SerializeField] private long _sequence;

        [IgnoreMember] public float When => _when;
        [Key(3), SerializeField] private float _when;

        public PlayerInputStates PushMove(Vector3 direction, long sequence, float when) {
            _direction = direction;
            _sequence = sequence;
            _when = when;

            Notify();
            return this;
        }

        public PlayerInputStates PushLook(Vector2 look, long sequence, float when) {
            _look = look;
            _sequence = sequence;
            _when = when;

            Notify();
            return this;
        }

        public PlayerInputStates PushCommands(InputCommand commands, long sequence, float when) {
            _commands = commands;
            _sequence = sequence;
            _when = when;

            Notify();
            return this;
        }

        public PlayerInputStates SyncGame(PlayerInputStates sync) {
            _commands = sync._commands;
            _direction = sync._direction;
            _look = sync._look;
            _sequence = sync._sequence;
            _when = sync._when;
            Notify();
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public class PlayersState : Notifiable<PlayersState> {
        [IgnoreMember] public IReadOnlyReactiveDictionary<int, PlayerState> Members => _members;
        [Key(0)] private ReactiveDictionary<int, PlayerState> _members = new ReactiveDictionary<int, PlayerState>();

        public PlayersState AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            _members[playerId] = new PlayerState(
                playerId: playerId,
                prefabKey: prefabKey,
                networkId: networkId
            );
            Notify();
            return this;
        }

        public PlayersState RemovePlayer(int playerId) {
            if (_members.Remove(playerId)) {
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
                _members[playerId].SyncGame(sync.Members[playerId]);
            }

            Notify();
            return this;
        }
    }

    public delegate int GetPlayerId();
}