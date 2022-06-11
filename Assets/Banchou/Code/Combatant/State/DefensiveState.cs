using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [Serializable, MessagePackObject]
    public record DefensiveState(
        bool IsInvincible = false,
        GuardStyle GuardStyle = GuardStyle.None,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<DefensiveState>(32) {
        [Key(0)][field: SerializeField] public bool IsInvincible { get; private set; } = IsInvincible;
        [Key(1)][field: SerializeField] public GuardStyle GuardStyle { get; private set; } = GuardStyle;
        [Key(2)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public override void Set(DefensiveState other) {
            IsInvincible = other.IsInvincible;
            GuardStyle = other.GuardStyle;
            LastUpdated = other.LastUpdated;
        }

        public DefensiveState Sync(
            float when,
            bool? isInvincible = null,
            GuardStyle? guardStyle = null,
            float? guardTime = null
        ) {
            LastUpdated = when;
            IsInvincible = isInvincible ?? IsInvincible;
            GuardStyle = guardStyle ?? GuardStyle;
            LastUpdated = when;
            return Notify(when);
        }

        public DefensiveState SetInvincibility(bool isInvincible, float when) {
            IsInvincible = isInvincible;
            LastUpdated = when;
            return Notify(when);
        }

        public DefensiveState Guard(GuardStyle style, float when) {
            GuardStyle = style;
            LastUpdated = when;
            return Notify(when);
        }
    }
}