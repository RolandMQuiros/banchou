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
}