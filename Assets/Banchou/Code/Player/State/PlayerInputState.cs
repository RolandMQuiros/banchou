using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    public enum PlayerStickState : byte {
        Neutral = 5,
        Forward = 6,
        ForwardRight = 3,
        Right = 2,
        BackRight = 1,
        Back = 4,
        BackLeft = 7,
        Left = 8,
        ForwardLeft = 9
    }
    
    [MessagePackObject, Serializable]
    public record PlayerInputState(
        int PlayerId = 0,
        InputCommand Commands = InputCommand.None,
        Vector3 Direction = new(),
        long Sequence = 0,
        float When = 0f
    ) : NotifiableWithHistory<PlayerInputState>(32) {
        [field: SerializeField] public int PlayerId { get; private set; } = PlayerId;
        [field: SerializeField] public InputCommand Commands { get; private set; } = Commands;
        [field: SerializeField] public Vector3 Direction { get; private set; } = Direction;

        // Look input is not shared across the network
        [field: SerializeField] public Vector2 Look { get; private set; }
        [field: SerializeField] public float When { get; private set; } = When;

        public override void Set(PlayerInputState other) {
            PlayerId = other.PlayerId;
            Commands = other.Commands;
            Direction = other.Direction;
            Look = other.Look;
            When = other.When;
        }

        public PlayerInputState Push(InputCommand commands, Vector3 direction, Vector2 look, float when) {
            Commands = commands;
            Direction = direction;
            Look = look;
            When = when;

            return Notify();
        }

        public PlayerInputState PushMove(Vector3 direction, float when) {
            Direction = direction;
            When = when;

            return Notify();
        }

        public PlayerInputState PushLook(Vector2 look, float when) {
            Look = look;
            When = when;

            return this;
        }

        public PlayerInputState PushCommands(InputCommand commands, float when) {
            Commands = commands;
            When = when;
            return Notify();
        }

        public PlayerInputState Sync(PlayerInputState sync) {
            Set(sync);
            return Notify();
        }
    }
}