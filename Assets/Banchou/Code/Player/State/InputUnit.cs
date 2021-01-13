using System.Collections;
using System.Collections.Generic;

using MessagePack;
using UnityEngine;

namespace Banchou.Player {

    public enum InputUnitType : byte {
        Command,
        Movement,
        Look
    }

    public enum PlayerCommand : byte {
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
        [Key(0)] public int PlayerId { get; private set; }
        [Key(1)] public InputUnitType Type { get; private set; }
        [Key(2)] public PlayerCommand Command { get; private set; }
        [Key(3)] public Vector3 Direction { get; private set; }
        [Key(4)] public long Sequence { get; private set; }
        [Key(5)] public float When { get; private set; }

        public InputUnit(
            int playerId,
            InputUnitType type,
            PlayerCommand command,
            Vector3 direction,
            long sequence,
            float when
        ) {
            PlayerId = playerId;
            Type = type;
            Command = command;
            Direction = direction;
            Sequence = sequence;
            When = when;
        }

        public InputUnit(
            int playerId,
            PlayerCommand command,
            long sequence,
            float when
        ) {
            PlayerId = playerId;
            Type = InputUnitType.Command;
            Command = command;
            Sequence = sequence;
            When = when;

            Direction = Vector3.zero;
        }

        public InputUnit(
            int playerId,
            Vector3 direction,
            long sequence,
            float when
        ) {
            PlayerId = playerId;
            Type = InputUnitType.Movement;
            Direction = direction;
            Sequence = sequence;
            When = when;

            Command = PlayerCommand.None;
        }

        public InputUnit(
            int playerId,
            Vector2 look,
            long sequence,
            float when
        ) {
            PlayerId = playerId;
            Type = InputUnitType.Look;
            Direction = look;
            Sequence = sequence;
            When = when;
            Command = PlayerCommand.None;
        }
    }
}