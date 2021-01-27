using System;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand : int {
        None = 0,
        LightAttack = 1,
        HeavyAttack = 2,
        LockOn = 4,
        LockOff = 8,
        ShortJump = 16,
        Jump = 256
    }
}