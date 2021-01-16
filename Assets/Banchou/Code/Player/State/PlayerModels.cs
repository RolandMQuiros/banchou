using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    [MessagePackObject, Serializable]
    public class PlayerState : Substate<PlayerState> {
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

        protected override void OnProcess() {
            Input.Process();
        }
    }

    [MessagePackObject, Serializable]
    public class PlayerInputStates : Substate<PlayerInputStates> {
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
    }

    [MessagePackObject, Serializable]
    public class PlayersState : Substate<PlayersState> {
        [Key(0)] public Dictionary<int, PlayerState> Members { get; private set; } = new Dictionary<int, PlayerState>();

        public PlayersState AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            Members[playerId] = new PlayerState(
                playerId: playerId,
                prefabKey: prefabKey,
                networkId: networkId
            );

            Notify();
            return this;
        }

        public PlayersState RemovePlayer(int playerId) {
            if (Members.Remove(playerId)) {
                Notify();
            }
            return this;
        }
    }

    public delegate int GetPlayerId();
}