using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record CombatantState(
        int PawnId,
        int Health = 0,
        CombatantStats Stats = null,
        DefensiveState Defense = null,
        AttackState Attack = null,
        HitState Hit = null,
        GrabState Grab = null,
        int LockOnTarget = 0,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<CombatantState>(32) {
        [Key(0)][field: SerializeField] public int PawnId { get; private set; } = PawnId;
        [Key(1)][field: SerializeField] public int Health { get; private set; } = Health;
        [Key(2)][field: SerializeField] public CombatantStats Stats { get; private set; } = Stats ?? new CombatantStats();
        [Key(3)][field: SerializeField] public DefensiveState Defense { get; private set; } = Defense ?? new DefensiveState();
        [Key(4)][field: SerializeField] public AttackState Attack { get; private set; } = Attack ?? new AttackState(PawnId);
        [Key(5)][field: SerializeField] public HitState Hit { get; private set; } = Hit ?? new HitState(PawnId);
        [Key(6)][field: SerializeField] public GrabState Grab { get; private set; } = Grab ?? new GrabState(PawnId);
        [Key(7)][field: SerializeField] public int LockOnTarget { get; private set; } = LockOnTarget;
        [Key(8)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public override void Set(CombatantState other) {
            Health = other.Health;
            Stats.Set(other.Stats);
            Defense.Set(other.Defense);
            Attack.Set(other.Attack);
            Hit.Set(other.Hit);
            Grab.Set(other.Grab);
            LockOnTarget = other.LockOnTarget;
            LastUpdated = other.LastUpdated;
        }

        public CombatantState Sync(CombatantState other) {
            Health = other.Health;
            Stats.Set(other.Stats);
            Attack.Sync(other.Attack);
            Hit.Sync(other.Hit);
            Grab.Sync(other.Grab);
            LockOnTarget = other.LockOnTarget;
            LastUpdated = other.LastUpdated;
            return Notify();
        }

        public CombatantState LockOn(int targetPawnId, float when) {
            if (LockOnTarget != targetPawnId) {
                LockOnTarget = targetPawnId;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public CombatantState LockOff(float when) {
            if (LockOnTarget != default) {
                LockOnTarget = default;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public CombatantState TakeHit(
            int attackerId,
            int attackId,
            HitStyle style,
            int damage,
            float pauseTime,
            float stunTime,
            Vector3 knockback,
            Vector3 contact,
            float when
        ) {
            if (Defense.IsInvincible) return this;
            Health = Mathf.Clamp(Health - damage, 0, Stats.MaxHealth);
            Hit.Take(attackerId, attackId, style, damage, pauseTime, stunTime, knockback, contact, when);
            Grab.Interrupt(when);
            LastUpdated = when;
            return Notify(when);
        }
    }
}