using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record HitState(
        int DefenderId,
        int AttackerId = default,
        int AttackId = default,
        int Damage = default,
        float PauseTime = default,
        float StunTime = default,
        Vector3 Knockback = default,
        float LastUpdated = default
    ) : NotifiableWithHistory<HitState>(32) {
        [Key(0)][field: SerializeField] public int DefenderId { get; private set; } = DefenderId;
        [Key(1)][field: SerializeField] public int AttackerId { get; private set; } = AttackerId;
        [Key(2)][field: SerializeField] public int AttackId { get; private set; } = AttackId;

        [Key(3)][field: SerializeField] public int Damage { get; private set; } = Damage;
        [Key(4)][field: SerializeField] public float PauseTime { get; private set; } = PauseTime;
        [Key(5)][field: SerializeField] public float StunTime { get; private set; } = StunTime;
        [Key(6)][field: SerializeField] public Vector3 Knockback { get; private set; } = Knockback;
        [Key(7)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public float PauseTimeAt(float now) => PauseTime - (now - LastUpdated);
        public float StunTimeAt(float now) => StunTime - (now - (LastUpdated + PauseTime));
        public bool IsStunned(float now) => StunTimeAt(now) > 0f;

        public override void Set(HitState other) {
            DefenderId = other.DefenderId;
            AttackerId = other.AttackerId;
            AttackId = other.AttackId;
            Damage = other.Damage;
            PauseTime = other.PauseTime;
            StunTime = other.StunTime;
            Knockback = other.Knockback;
            LastUpdated = other.LastUpdated;
        }

        public HitState Take(int attackerId, int attackId, int damage, float pauseTime, float stunTime, float when) {
            if (AttackerId != attackerId || AttackId != attackId) {
                AttackerId = attackerId;
                AttackId = attackId;
                Damage = damage;
                PauseTime = pauseTime;
                StunTime = stunTime;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public HitState Cancel(float when) {
            if (AttackerId != default) {
                AttackerId = default;
                AttackId = default;
                Damage = 0;
                PauseTime = 0f;
                StunTime = 0f;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }
    }
}