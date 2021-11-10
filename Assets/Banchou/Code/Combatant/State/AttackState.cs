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
    public record AttackState(
        int AttackId = 0,
        AttackPhase Phase = AttackPhase.Neutral,
        int TargetId = 0,
        int Damage = 0,
        Vector3 Recoil = new(),
        float LastUpdated = 0f
    ) : NotifiableWithHistory<AttackState>(32) {
        [field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [field: SerializeField] public AttackPhase Phase { get; private set; } = Phase; 
        [field: SerializeField] public int TargetId { get; private set; } = TargetId;
        [field: SerializeField] public int Damage { get; private set; } = Damage;
        [field: SerializeField] public Vector3 Recoil { get; private set; } = Recoil;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;
        
        public override void Set(AttackState other) {
            AttackId = other.AttackId;
            TargetId = other.TargetId;
            Phase = other.Phase;
            Damage = other.Damage;
            Recoil = other.Recoil;
            LastUpdated = other.LastUpdated;
        }

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