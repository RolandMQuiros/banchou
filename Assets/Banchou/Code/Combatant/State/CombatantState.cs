using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public class CombatantState : NotifiableWithHistory<CombatantState> {
        [Key(0)] public readonly CombatantStats Stats;
        [Key(1)][field: SerializeField] public int Health { get; private set; }
        [Key(2)] public readonly DefensiveState Defense = new DefensiveState();
        [Key(3)] public readonly HitState LastHit = new HitState();
        [Key(4)] public readonly AttackState Attack = new AttackState();
        [Key(5)][field: SerializeField] public float LastUpdated { get; private set; }
        
        #region Boilerplate
        [SerializationConstructor]
        public CombatantState(
            CombatantStats stats,
            int health,
            DefensiveState defense,
            HitState lastHit,
            AttackState attack,
            float lastUpdated
        ) : base(32) {
            Stats = stats;
            Health = health;
            Defense = defense;
            LastHit = lastHit;
            Attack = attack;
            LastUpdated = lastUpdated;
        }

        public CombatantState(int maxHealth) : base(32) {
            Stats = new CombatantStats(maxHealth, 1f);
            Health = maxHealth;
        }

        public CombatantState() : base(32) {
            Stats = new CombatantStats(100, 1f);
            Health = 100;
        }
        
        public override void Set(CombatantState other) {
            Stats.Set(other.Stats);
            Health = other.Health;
            Defense.Set(other.Defense);
            LastHit.Set(other.LastHit);
            Attack.Set(other.Attack);
            LastUpdated = other.LastUpdated;
        }
        #endregion 

        public CombatantState Hit(
            int attackerId,
            Vector3 pawnDirection,
            Vector3 attackDirection,
            Vector3 knockback,
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
            LastHit.Hit(attackerId, damage, knockback, hitStun, true, when);
            Defense.Set(guardTime: guardTime, when: when);

            return Notify(when);
        }
    }
}