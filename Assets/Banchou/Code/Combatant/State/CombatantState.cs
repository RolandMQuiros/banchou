using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public class CombatantState : Notifiable<CombatantState>, IRecordable<CombatantState> {
        public const float KnockbackDeceleration = 10f;
        [Key(0)][field: SerializeField] public int Health { get; private set; }
        [Key(1)][field: SerializeField] public int MaxHealth { get; private set; }
        [Key(2)][field: SerializeField] public CombatantAttackPhase AttackPhase { get; private set; }
        [Key(3)][field: SerializeField] public float GuardTime { get; private set; }
        [Key(4)][field: SerializeField] public float StunTime { get; private set; }
        [Key(5)][field: SerializeField] public bool IsCountered { get; private set; }
        [Key(6)][field: SerializeField] public Vector3 Knockback { get; private set; }
        [Key(7)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializeField]
        private History<CombatantState> _history = new History<CombatantState>(32, () => new CombatantState());
        
        public float GuardTimeAt(float when) => Mathf.Max(GuardTime - when - LastUpdated, 0f);
        public float StunTimeAt(float when) => Mathf.Max(StunTime - when - LastUpdated, 0f);
        public bool IsCounteredAt(float when) => IsCountered && StunTimeAt(when) > 0f;
        public Vector3 KnockbackAt(float when) => Vector3.MoveTowards(
            Knockback,
            Vector3.zero,
            (when - LastUpdated) * KnockbackDeceleration
        );

        [SerializationConstructor]
        public CombatantState(
            int health,
            int maxHealth,
            CombatantAttackPhase attackPhase,
            float guardTime,
            float stunTime,
            bool isCountered,
            Vector3 knockback,
            float lastUpdated
        ) {
            Health = health;
            MaxHealth = maxHealth;
            AttackPhase = attackPhase;
            GuardTime = guardTime;
            StunTime = stunTime;
            IsCountered = isCountered;
            Knockback = knockback;
            LastUpdated = lastUpdated;
        }

        public CombatantState(int maxHealth) {
            Health = maxHealth;
            MaxHealth = maxHealth;
        }
        
        public CombatantState() { }

        public CombatantState SetAttackPhase(CombatantAttackPhase phase, float when) {
            AttackPhase = phase;
            LastUpdated = when;
            return Notify();
        }

        public CombatantState Hit(
            Vector3 pawnDirection,
            Vector3 attackDirection,
            Vector3 knockback,
            float hitStun,
            int damage,
            float when
        ) {
            UpdateTimers(when);
            if (GuardTime > 0f && Vector3.Dot(pawnDirection, attackDirection) < 0f) {
                damage /= 2;
                knockback /= 2f;
                GuardTime -= damage * 0.1f;
            } else if (AttackPhase == CombatantAttackPhase.Active) {
                IsCountered = true;
                StunTime = hitStun * 2f;
            } else {
                StunTime = hitStun;
            }
            
            Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
            Knockback = knockback;

            return Notify();
        }

        public CombatantState Guard(float guardTime, float when) {
            UpdateTimers(when);
            if (AttackPhase == CombatantAttackPhase.Neutral) {
                GuardTime = guardTime;
            }
            return Notify();
        }

        public CombatantState Unguard(float when) {
            UpdateTimers(when);
            if (GuardTime > 0f) {
                GuardTime = 0f;
            }
            return Notify();
        }

        private CombatantState UpdateTimers(float when) {
            GuardTime = GuardTimeAt(when);
            StunTime = StunTimeAt(when);
            IsCountered = IsCounteredAt(when);
            Knockback = KnockbackAt(when);
            LastUpdated = when;
            return this;
        }
        
        #region Rewindable
        protected override CombatantState Notify() {
            _history.PushFrame(this, LastUpdated);
            return base.Notify();
        }

        public void Set(CombatantState other) {
            Health = other.Health;
            MaxHealth = other.MaxHealth;
            AttackPhase = other.AttackPhase;
            GuardTime = other.GuardTime;
            StunTime = other.StunTime;
            IsCountered = other.IsCountered;
            Knockback = other.Knockback;
            LastUpdated = other.LastUpdated;
        }
        #endregion
    }
    
    public enum CombatantAttackPhase {
        Neutral,
        Starting,
        Active,
        Recover
    }
}