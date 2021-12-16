using System;

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
        HeavyAttack = 1 << 11,
        HeavyAttackHold = 1 << 12,
        LockOn = 1 << 13,
        LockOff = 1 << 14,
        ShortJump = 1 << 15,
        Jump = 1 << 16
    }

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