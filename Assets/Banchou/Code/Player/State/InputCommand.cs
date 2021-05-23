using System;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand : int {
        None = 0,
        LightAttack = 1,
        HeavyAttack = 2,
        LockOn = 4,
        LockOff = 8,
        ToggleLock = 16,
        ShortJump = 32,
        Jump = 64
    }
}