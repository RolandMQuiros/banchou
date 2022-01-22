using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record CombatantState(
        CombatantStats Stats = null,
        int Health = 0,
        DefensiveState Defense = null,
        HitState LastHit = null,
        AttackState Attack = null,
        int LockOnTarget = 0,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<CombatantState>(32) {
        [field: SerializeField] public CombatantStats Stats { get; private set; } = Stats ?? new CombatantStats();
        [field: SerializeField] public int Health { get; private set; } = Health;
        [field: SerializeField] public DefensiveState Defense { get; private set; } = Defense ?? new DefensiveState();
        [field: SerializeField] public HitState LastHit { get; private set; } = LastHit ?? new HitState();
        [field: SerializeField] public AttackState Attack { get; private set; } = Attack ?? new AttackState();
        [field: SerializeField] public int LockOnTarget { get; private set; } = LockOnTarget;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public override void Set(CombatantState other) {
            Stats.Set(other.Stats);
            Health = other.Health;
            Defense.Set(other.Defense);
            LastHit.Set(other.LastHit);
            Attack.Set(other.Attack);
            LastUpdated = other.LastUpdated;
        }

        public CombatantState LockOn(int targetPawnId, float when) {
            if (LockOnTarget != targetPawnId) {
                LockOnTarget = targetPawnId;
                return Notify(when);
            }
            return this;
        }

        public CombatantState LockOff(float when) {
            if (LockOnTarget != default) {
                LockOnTarget = default;
                return Notify(when);
            }
            return this;
        }

        public CombatantState Hit(
            int attackerId,
            int attackId,
            Vector3 contact,
            Vector3 pawnDirection,
            Vector3 attackDirection,
            Vector3 knockback,
            float hitPause,
            float hitStun,
            int damage,
            float when
        ) {
            if (Defense.IsInvincible) return this;

            var guardTime = Defense.GuardTime;
            if (Defense.GuardTime > 0f && Vector3.Dot(pawnDirection, attackDirection) < 0f) {
                damage /= 2;
                knockback /= 2f;
                guardTime -= damage * 0.1f;
            } else if (Attack.Phase == AttackPhase.Active) {
                hitStun *= 2f;
            }
            
            Health = Mathf.Clamp(Health - damage, 0, Stats.MaxHealth);
            LastHit.Hit(attackerId, attackId, contact, damage, knockback, hitPause, hitStun, when);
            Defense.Set(guardTime: guardTime, when: when);

            return Notify(when);
        }
    }
}