using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantAttackState : Notifiable<CombatantAttackState>{
        [Key(0)][field: SerializeField]
        public int AttackId { get; private set; } = 0;

        [Key(1)][field: SerializeField]
        public CombatantAttackPhase Phase { get; private set; } = CombatantAttackPhase.Neutral;

        [Key(2)][field: SerializeField]
        public float LastUpdated { get; private set; } = 0f;


        [SerializationConstructor]
        public CombatantAttackState(int attackId, CombatantAttackPhase phase, float lastUpdated) {
            AttackId = attackId;
            Phase = phase;
            LastUpdated = lastUpdated;
        }

        public CombatantAttackState Start(int attackId, float when) {
            AttackId = attackId;
            Phase = CombatantAttackPhase.Starting;
            LastUpdated = when;
            return Notify();
        }

        public CombatantAttackState Activate(float when) {
            Phase = CombatantAttackPhase.Active;
            LastUpdated = when;
            return Notify();
        }

        public CombatantAttackState Recover(float when) {
            Phase = CombatantAttackPhase.Recover;
            LastUpdated = when;
            return Notify();
        }

        public CombatantAttackState Finish(float when) {
            AttackId = 0;
            Phase = CombatantAttackPhase.Neutral;
            LastUpdated = when;
            return Notify();
        }
    }

    public enum CombatantAttackPhase {
        Neutral,
        Starting,
        Active,
        Recover
    }
}