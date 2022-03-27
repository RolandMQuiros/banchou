using System;
using UnityEngine;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand {
        None = 0,
        Neutral = 1,
        Forward = 1 << 1,
        ForwardRight = 1 << 2,
        Right = 1 << 3,
        BackRight = 1 << 4,
        Back = 1 << 5,
        BackLeft = 1 << 6,
        Left = 1 << 7,
        ForwardLeft = 1 << 8,
        LightAttack = 1 << 9,
        LightAttackHold = 1 << 10,
        LightAttackUp = 1 << 11,
        HeavyAttack = 1 << 12,
        HeavyAttackHold = 1 << 13,
        HeavyAttackUp = 1 << 14,
        SpecialAttack = 1 << 15,
        SpecialAttackHold = 1 << 16,
        SpecialAttackUp = 1 << 17,
        LockOn = 1 << 18,
        LockOff = 1 << 19,
        ShortJump = 1 << 20,
        Jump = 1 << 21,
        Block = 1 << 22,
        Unblock = 1 << 23
    }

    [Serializable]
    public class InputGestureStep {
        public float Delay;
        public bool RequireConfirm;
        public InputCommand Command;
    };

    [Serializable]
    public class InputGesture {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public InputGestureStep[] Sequence { get; private set; }
    };

    public static class InputCommandMasks {
        public const InputCommand Stick = 
            InputCommand.Neutral |
            InputCommand.Forward |
            InputCommand.ForwardRight |
            InputCommand.Right |
            InputCommand.BackRight |
            InputCommand.Back |
            InputCommand.BackLeft |
            InputCommand.Left |
            InputCommand.ForwardLeft;

        public const InputCommand Attacks =
            InputCommand.LightAttack |
            InputCommand.LightAttackHold |
            InputCommand.HeavyAttack |
            InputCommand.HeavyAttackHold;

        public const InputCommand Gestures = Stick | Attacks;
    }
}