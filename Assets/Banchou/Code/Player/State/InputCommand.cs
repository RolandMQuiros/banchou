using System;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand : int {
        None = 0,
        Forward = 1,
        ForwardRight = 1 << 1,
        Right = 1 << 2,
        BackRight = 1 << 3,
        Back = 1 << 4,
        BackLeft = 1 << 5,
        Left = 1 << 6,
        ForwardLeft = 1 << 7,
        LightAttack = 1 << 8,
        LightAttackHold = 1 << 9,
        HeavyAttack = 1 << 10,
        HeavyAttackHold = 1 << 11,
        LockOn = 1 << 12,
        LockOff = 1 << 13,
        ShortJump = 1 << 14,
        Jump = 1 << 15
    }

    public static class InputCommandMasks {
        public const InputCommand Stick = InputCommand.Forward |
            InputCommand.ForwardRight |
            InputCommand.Right |
            InputCommand.BackRight |
            InputCommand.Back |
            InputCommand.BackLeft |
            InputCommand.Left |
            InputCommand.ForwardLeft;

        public const InputCommand Attacks = InputCommand.LightAttack |
            InputCommand.LightAttackHold |
            InputCommand.HeavyAttack |
            InputCommand.HeavyAttackHold;

        public const InputCommand Gestures = Stick | Attacks;
    }
}