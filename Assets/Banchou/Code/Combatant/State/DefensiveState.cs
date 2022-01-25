using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public record DefensiveState(
        float InvincibilityTime = 0f,
        GuardStyle GuardStyle = GuardStyle.None,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<DefensiveState>(32) {
        [field: SerializeField] public float InvincibilityTime { get; private set; } = InvincibilityTime;
        [field: SerializeField] public GuardStyle GuardStyle { get; private set; } = GuardStyle;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public float InvincibilityTimeAt(float when) => InvincibilityTime - when - LastUpdated;
        public override void Set(DefensiveState other) {
            InvincibilityTime = other.InvincibilityTime;
            LastUpdated = other.LastUpdated;
        }

        public DefensiveState Set(
            float when,
            bool? isInvincible = null,
            GuardStyle? guardStyle = null,
            float? guardTime = null
        ) {
            LastUpdated = when;
            GuardStyle = guardStyle ?? GuardStyle;
            return UpdateTimers(when);
        }

        public DefensiveState SetInvincibility(float time, float when) {
            InvincibilityTime = time;
            return UpdateTimers(when);
        }

        public DefensiveState Guard(GuardStyle style, float when) {
            GuardStyle = style;
            return UpdateTimers(when);
        }
        
        public DefensiveState Set

        private DefensiveState UpdateTimers(float when) {
            var diff = when - LastUpdated;
            InvincibilityTime = when - LastUpdated;
            LastUpdated = when;
            return Notify(when);
        }
    }
}