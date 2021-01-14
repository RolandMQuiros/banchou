using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand : byte {
        None,
        LightAttack,
        HeavyAttack,
        LockOn,
        LockOff,
        ShortJump,
        Jump
    }

    [MessagePackObject]
    public struct InputUnit {
        public static readonly InputUnit Empty = default;

        [Key(0)] public int PlayerId { get; private set; }
        [Key(2)] public InputCommand Commands { get; private set; }
        [Key(3)] public Vector3 Direction { get; private set; }
        [IgnoreMember] public Vector2 Look { get; private set; }
        [Key(4)] public long Sequence { get; private set; }
        [Key(5)] public float When { get; private set; }

        public InputUnit(
            int playerId,
            long sequence,
            float when,
            InputCommand commands = InputCommand.None,
            Vector3 direction = default,
            Vector2 look = default
        ) {
            PlayerId = playerId;
            Sequence = sequence;
            When = when;
            Commands = commands;
            Direction = direction;
            Look = look;
        }
    }
}