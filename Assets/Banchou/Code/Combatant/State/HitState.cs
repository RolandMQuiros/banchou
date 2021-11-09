using MessagePack;
using UnityEngine;

namespace Banchou {
    [MessagePackObject]
    public class HitState : NotifiableWithHistory<HitState> {
        [Key(0)][field: SerializeField] public int AttackerId { get; private set; } = default;
        [Key(1)][field: SerializeField] public int Damage { get; private set; } = default;
        [Key(2)][field: SerializeField] public Vector3 Knockback { get; private set; }
        [Key(3)][field: SerializeField] public float StunTime { get; private set; } = 0f;
        [Key(4)][field: SerializeField] public bool IsCountered { get; private set; } = false;
        [Key(5)][field: SerializeField] public float LastUpdated { get; private set; } = 0f;
        
        #region Boilerplate
        [SerializationConstructor]
        public HitState(
            int attackerId,
            int damage,
            Vector3 knockback,
            float stunTime,
            bool isCountered,
            float lastUpdated
        ) : base(32) {
            AttackerId = attackerId;
            Damage = damage;
            Knockback = knockback;
            StunTime = stunTime;
            IsCountered = isCountered;
            LastUpdated = lastUpdated;
        }
        public HitState() : base(32) { }
        
        public override void Set(HitState other) {
            AttackerId = other.AttackerId;
            Knockback = other.Knockback;
            StunTime = other.StunTime;
            IsCountered = other.IsCountered;
            LastUpdated = other.LastUpdated;
        }
        #endregion

        public float StunTimeAt(float when) => StunTime - (when - LastUpdated);
        public float NormalizedStunTimeAt(float when) => (when - LastUpdated) / StunTime;
        public bool IsStunned(float when) => StunTimeAt(when) > 0f;
        
        public HitState Hit(
            int attackerId,
            int damage,
            Vector3 knockback,
            float stunTime,
            bool isCountered,
            float when
        ) {
            AttackerId = attackerId;
            Damage = damage;
            Knockback = knockback;
            StunTime = stunTime;
            IsCountered = isCountered;
            LastUpdated = when;
            return Notify(when);
        }
    }
}