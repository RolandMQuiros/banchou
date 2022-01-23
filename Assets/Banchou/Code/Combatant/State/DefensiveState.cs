using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public record DefensiveState(
        bool IsInvincible = false,
        GuardStyle GuardStyle = GuardStyle.None,
        float GuardTime = 0f,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<DefensiveState>(32) {
        [field: SerializeField] public bool IsInvincible { get; private set; } = IsInvincible;
        [field: SerializeField] public GuardStyle GuardStyle { get; private set; } = GuardStyle;
        [field: SerializeField] public float GuardTime { get; private set; } = GuardTime;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;
        
        public override void Set(DefensiveState other) {
            IsInvincible = other.IsInvincible;
            GuardTime = other.GuardTime;
            LastUpdated = other.LastUpdated;
        }

        public DefensiveState Set(
            float when,
            bool? isInvincible = null,
            GuardStyle? guardStyle = null,
            float? guardTime = null
        ) {
            LastUpdated = when;
            IsInvincible = isInvincible ?? IsInvincible;
            GuardStyle = guardStyle ?? GuardStyle;
            GuardTime = guardTime ?? GuardTime;
            return UpdateTimers(when);
        }

        public DefensiveState SetInvincibility(bool isInvincible, float when) {
            IsInvincible = isInvincible;
            return UpdateTimers(when);
        }

        public DefensiveState Guard(GuardStyle style, float guardTime, float when) {
            GuardTime = guardTime;
            return UpdateTimers(when);
        }

        private DefensiveState UpdateTimers(float when) {
            var diff = when - LastUpdated;
            GuardTime = Mathf.Min(0, GuardTime - diff);
            LastUpdated = when;
            return Notify(when);
        }
    }
}