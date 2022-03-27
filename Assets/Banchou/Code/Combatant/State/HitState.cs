using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record HitState(
        int DefenderId,
        int AttackerId = default,
        int AttackId = default,
        HitStyle Style = default,
        int Damage = default,
        float PauseTime = default,
        float StunTime = default,
        Vector3 Knockback = default,
        Vector3 Contact = default,
        float LastUpdated = default
    ) : NotifiableWithHistory<HitState>(32) {
        [Key(0)][field: SerializeField] public int DefenderId { get; private set; } = DefenderId;
        [Key(1)][field: SerializeField] public int AttackerId { get; private set; } = AttackerId;
        [Key(2)][field: SerializeField] public int AttackId { get; private set; } = AttackId;

        [Key(3)][field: SerializeField] public HitStyle Style { get; private set; } = Style;
        [Key(4)][field: SerializeField] public int Damage { get; private set; } = Damage;
        [Key(5)][field: SerializeField] public float PauseTime { get; private set; } = PauseTime;
        [Key(6)][field: SerializeField] public float StunTime { get; private set; } = StunTime;
        [Key(7)][field: SerializeField] public Vector3 Knockback { get; private set; } = Knockback;
        [Key(8)][field: SerializeField] public Vector3 Contact { get; private set; } = Contact;
        [Key(9)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public float NormalizedPauseTime(float timeScale, float when) =>
            Mathf.Clamp01((when - LastUpdated) * timeScale / PauseTime);
        
        public float NormalizedStunTime(float timeScale, float when) =>
            Mathf.Clamp01((when - LastUpdated) * timeScale / (PauseTime + StunTime));
        
        public override void Set(HitState other) {
            DefenderId = other.DefenderId;
            AttackerId = other.AttackerId;
            AttackId = other.AttackId;
            Style = other.Style;
            Damage = other.Damage;
            PauseTime = other.PauseTime;
            StunTime = other.StunTime;
            Knockback = other.Knockback;
            Contact = other.Contact;
            LastUpdated = other.LastUpdated;
        }

        public HitState Sync(HitState other) {
            Set(other);
            return Notify();
        }

        public HitState Take(int attackerId, int attackId, HitStyle style, int damage, float pauseTime,
            float stunTime, Vector3 knockback, Vector3 contact, float when)
        {
            if (AttackerId != attackerId || AttackId != attackId) {
                AttackerId = attackerId;
                AttackId = attackId;
                Style = style;
                Damage = damage;
                PauseTime = pauseTime;
                StunTime = stunTime;
                Knockback = knockback;
                Contact = contact;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public HitState Cancel(float when) {
            if (AttackerId != default) {
                AttackerId = default;
                AttackId = default;
                Style = HitStyle.None;
                Damage = 0;
                PauseTime = 0f;
                StunTime = 0f;
                Knockback = Vector3.zero;
                Contact = Vector3.zero;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }
    }

    public enum HitStyle {
        None,
        Confirmed,
        Blocked,
        Grabbed
    }
}