using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    
    
    [MessagePackObject]
    public class DefensiveState : NotifiableWithHistory<DefensiveState> {
        [field: Key(0), SerializeField] public bool IsInvincible { get; private set; }
        [field: Key(1), SerializeField] public float GuardTime { get; private set; }
        [field: Key(2), SerializeField] public float LastUpdated { get; private set; }

        #region Boilerplate
        [SerializationConstructor]
        public DefensiveState(
            bool isInvincible,
            float guardTime,
            float lastUpdated
        ) : base(32) {
            IsInvincible = isInvincible;
            GuardTime = guardTime;
            LastUpdated = lastUpdated;
        }
        
        public DefensiveState() : base(32) { }

        public override void Set(DefensiveState other) {
            IsInvincible = other.IsInvincible;
            GuardTime = other.GuardTime;
            LastUpdated = other.LastUpdated;
        }
        #endregion

        public DefensiveState Set(
            float when,
            bool? isInvincible = null,
            float? guardTime = null
        ) {
            LastUpdated = when;
            IsInvincible = isInvincible ?? IsInvincible;
            GuardTime = guardTime ?? GuardTime;
            return Notify(when);
        }

        public DefensiveState SetInvincibility(bool isInvincible, float when) {
            IsInvincible = isInvincible;
            LastUpdated = when;
            return Notify(when);
        }

        public DefensiveState Guard(float guardTime, float when) {
            GuardTime = guardTime;
            return Notify(when);
        }
    }
}