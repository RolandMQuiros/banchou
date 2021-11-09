using MessagePack;
using UnityEngine;

namespace Banchou {
    [MessagePackObject]
    public record HitState(
        int AttackerId = 0, int Damage = 0, Vector3 Knockback = new Vector3(), float StunTime = 0f,
        bool IsCountered = false, float LastUpdated = 0f
    ) : NotifiableRecordWithHistory<HitState>(32) {
        [field: SerializeField] public int AttackerId { get; private set; } = AttackerId;
        [field: SerializeField] public int Damage { get; private set; } = Damage;
        [field: SerializeField] public Vector3 Knockback { get; private set; } = Knockback;
        [field: SerializeField] public float StunTime { get; private set; } = StunTime;
        [field: SerializeField] public bool IsCountered { get; private set; } = IsCountered;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;
        
        public override void Set(HitState other) {
            AttackerId = other.AttackerId;
            Knockback = other.Knockback;
            StunTime = other.StunTime;
            IsCountered = other.IsCountered;
            LastUpdated = other.LastUpdated;
        }

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