using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    public enum AttackPhase : byte {
        Neutral,
        Starting,
        Active,
        Recover
    }
    
    [MessagePackObject]
    public class AttackState : NotifiableWithHistory<AttackState> {
        [Key(0)][field: SerializeField]public int AttackId { get; private set; }
        [Key(1)][field: SerializeField]public AttackPhase Phase { get; private set; } 
        [Key(2)][field: SerializeField]public int TargetId { get; private set; }
        [Key(3)][field: SerializeField]public int Damage { get; private set; }
        [Key(4)][field: SerializeField]public Vector3 Recoil { get; private set; }
        [Key(5)][field: SerializeField]public float LastUpdated { get; private set; }
        
        #region Boilerplate
        public AttackState() : base(32) { }
        
        [SerializationConstructor]
        public AttackState(
            int attackId,
            AttackPhase phase,
            int targetId,
            int damage,
            Vector3 recoil,
            float lastUpdated
        ) : base(32) {
            AttackId = attackId;
            TargetId = targetId;
            Phase = phase;
            Damage = damage;
            Recoil = recoil;
            LastUpdated = lastUpdated;
        }
        
        public override void Set(AttackState other) {
            AttackId = other.AttackId;
            TargetId = other.TargetId;
            Phase = other.Phase;
            Damage = other.Damage;
            Recoil = other.Recoil;
            LastUpdated = other.LastUpdated;
        }
        #endregion

        public AttackState Start(float when) {
            AttackId++;
            Phase = AttackPhase.Starting;
            LastUpdated = when;
            return Notify(when);
        }

        public AttackState Activate(float when) {
            Phase = AttackPhase.Active;
            LastUpdated = when;
            return Notify(when);
        }

        public AttackState Recover(float when) {
            Phase = AttackPhase.Recover;
            LastUpdated = when;
            return Notify(when);
        }

        public AttackState Finish(float when) {
            Phase = AttackPhase.Neutral;
            LastUpdated = when;
            return Notify(when);
        }
        
        public AttackState Connect(int targetId, int damage, Vector3 recoil, float when) {
            TargetId = targetId;
            Damage = damage;
            Recoil = recoil;
            LastUpdated = when;
            return Notify(when);
        }
    }
}