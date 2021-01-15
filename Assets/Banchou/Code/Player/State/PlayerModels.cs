using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    [MessagePackObject]
    public class PlayerState : Substate<PlayerState> {
        [Key(0)] public readonly int PlayerId;
        [Key(1)] public readonly string PrefabKey;
        [Key(2)] public int NetworkId { get; private set; }
        [Key(3)] public PlayerInputStates Input { get; private set; } = new PlayerInputStates();

        public PlayerState() { }
        public PlayerState(
            int id,
            string prefabKey,
            int networkId = 0
        ) {
            PlayerId = id;
            PrefabKey = prefabKey;
            NetworkId = networkId;
        }

        protected override void OnProcess() {
            Input.Process();
        }
    }

    [MessagePackObject]
    public class PlayerInputStates : Substate<PlayerInputStates> {
        [Key(0)] public InputCommand Commands { get; private set; }
        [Key(1)] public Vector3 Direction { get; private set; }
        [IgnoreMember] public Vector2 Look { get; private set; }
        [Key(2)] public long Sequence { get; private set; }
        [Key(3)] public float When { get; private set; }

        public void PushMove(Vector3 direction, long sequence, float when) {
            Direction = direction;
            Sequence = sequence;
            When = when;
            Notify();
        }

        public void PushLook(Vector2 look, long sequence, float when) {
            Look = look;
            Sequence = sequence;
            When = when;
            Notify();
        }

        public void PushCommands(InputCommand commands, long sequence, float when) {
            Commands = commands;
            Sequence = sequence;
            When = when;
            Notify();
        }
    }

    [MessagePackObject]
    public class PlayersState : Substate<PlayersState> {
        [Key(0)] public Dictionary<int, PlayerState> Members { get; private set; } = new Dictionary<int, PlayerState>();

        public void AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            Members[playerId] = new PlayerState(
                id: playerId,
                prefabKey: prefabKey,
                networkId: networkId
            );
            Notify();
        }

        public void RemovePlayer(int playerId) {
            if (Members.Remove(playerId)) {
                Notify();
            }
        }
    }

    public delegate int GetPlayerId();
}