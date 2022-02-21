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
        GrabState Grab = null,
        int LockOnTarget = 0,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<CombatantState>(32) {
        [field: SerializeField] public int PawnId { get; private set; } = PawnId;
        [field: SerializeField] public int Health { get; private set; } = Health;
        [field: SerializeField] public CombatantStats Stats { get; private set; } = Stats ?? new CombatantStats();
        [field: SerializeField] public DefensiveState Defense { get; private set; } = Defense ?? new DefensiveState();
        [field: SerializeField] public AttackState Attack { get; private set; } = Attack ?? new AttackState(PawnId);
        [field: SerializeField] public GrabState Grab { get; private set; } = Grab ?? new GrabState(PawnId);
        [field: SerializeField] public int LockOnTarget { get; private set; } = LockOnTarget;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public override void Set(CombatantState other) {
            Health = other.Health;
            Stats.Set(other.Stats);
            Defense.Set(other.Defense);
            Attack.Set(other.Attack);
            Grab.Set(other.Grab);
            LockOnTarget = other.LockOnTarget;
            LastUpdated = other.LastUpdated;
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

        public CombatantState Hit(
            int damage,
            float when
        ) {
            if (Defense.IsInvincible) return this;
            Health = Mathf.Clamp(Health - damage, 0, Stats.MaxHealth);
            LastUpdated = when;
            return Notify(when);
        }
    }
}