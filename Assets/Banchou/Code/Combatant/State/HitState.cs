using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [Serializable, MessagePackObject(true)]
    public record HitState(
        int AttackerId = 0,
        int AttackId = 0,
        Vector3 Contact = new(),
        bool Blocked = false,
        int Damage = 0,
        Vector3 Knockback = new(),
        float PauseTime = 0f,
        float StunTime = 0f,
        bool IsGrabbed = false,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<HitState>(32) {
        [field: SerializeField] public int AttackerId { get; private set; } = AttackerId;
        [field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [field: SerializeField] public Vector3 Contact { get; private set; } = Contact;
        [field: SerializeField] public bool Blocked { get; private set; } = Blocked;
        [field: SerializeField] public int Damage { get; private set; } = Damage;
        [field: SerializeField] public Vector3 Knockback { get; private set; } = Knockback;
        [field: SerializeField] public float PauseTime { get; private set; } = PauseTime;
        [field: SerializeField] public float StunTime { get; private set; } = StunTime;
        [field: SerializeField] public bool IsGrabbed { get; private set; } = IsGrabbed;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;
        
        public override void Set(HitState other) {
            AttackerId = other.AttackerId;
            AttackId = other.AttackId;
            Contact = other.Contact;
            Knockback = other.Knockback;
            PauseTime = other.PauseTime;
            StunTime = other.StunTime;
            IsGrabbed = other.IsGrabbed;
            LastUpdated = other.LastUpdated;
        }

        public float StunTimeAt(float when) => StunTime - (when - LastUpdated + PauseTime);
        
        public float NormalizedStunTimeAt(float when) => Mathf.Approximately(StunTime, 0f) ? 1f :
            Mathf.Clamp01((when - (LastUpdated + PauseTime)) / StunTime);

        public float PauseTimeAt(float when) => PauseTime - (when - LastUpdated);
        
        public float NormalizedPauseTimeAt(float when) => Mathf.Approximately(PauseTime, 0f) ? 1f :
            Mathf.Clamp01((when - LastUpdated) / PauseTime);
        
        public bool IsStunned(float when) => StunTimeAt(when) > 0f;
        
        public HitState Hit(
            int attackerId,
            int attackId,
            Vector3 contact,
            bool blocked,
            int damage,
            Vector3 knockback,
            float pauseTime,
            float stunTime,
            bool isGrabbed,
            float when
        ) {
            AttackerId = attackerId;
            AttackId = attackId;
            Contact = contact;
            Blocked = blocked;
            Damage = damage;
            Knockback = knockback;
            PauseTime = pauseTime;
            StunTime = stunTime;
            IsGrabbed = isGrabbed;
            LastUpdated = when;
            return Notify(when);
        }
    }
}