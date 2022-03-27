using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    public enum AttackPhase : byte {
        Neutral,
        Starting,
        Active,
        Recover
    }

    public enum AttackStyle : byte {
        Strike,
        Grab
    }
    
    [Serializable, MessagePackObject]
    public record AttackState(
        int AttackerId,
        int AttackId = 0,
        AttackPhase Phase = AttackPhase.Neutral,
        int TargetId = 0,
        float WhenHit = 0f,
        int Damage = 0,
        HitStyle HitStyle = HitStyle.None,
        float PauseTime = 0f,
        Vector3 Contact = new(),
        Vector3 Recoil = new(),
        float LastUpdated = 0f
    ) : NotifiableWithHistory<AttackState>(32) {
        [Key(0)][field: SerializeField] public int AttackerId { get; } = AttackerId;
        [Key(1)][field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [Key(2)][field: SerializeField] public AttackPhase Phase { get; private set; } = Phase; 
        
        [Key(3)][field: SerializeField] public int TargetId { get; private set; } = TargetId;
        [Key(4)][field: SerializeField] public float WhenHit { get; private set; } = WhenHit;
        [Key(5)][field: SerializeField] public int Damage { get; private set; } = Damage;
        [Key(6)][field: SerializeField] public HitStyle HitStyle { get; private set; } = HitStyle;
        [Key(7)][field: SerializeField] public float PauseTime { get; private set; } = PauseTime;

        [Key(8)][field: SerializeField] public Vector3 Contact { get; private set; } = Contact;
        [Key(9)][field: SerializeField] public Vector3 Recoil { get; private set; } = Recoil;
        
        [Key(10)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public float NormalizedPauseTime(float timeScale, float when) =>
            Mathf.Clamp01((when - WhenHit) * timeScale / PauseTime);

        public override void Set(AttackState other) {
            AttackId = other.AttackId;
            Phase = other.Phase;
            
            TargetId = other.TargetId;
            WhenHit = other.WhenHit;
            Damage = other.Damage;
            HitStyle = other.HitStyle;
            PauseTime = other.PauseTime;
            Contact = other.Contact;
            Recoil = other.Recoil;
            
            LastUpdated = other.LastUpdated;
        }

        public AttackState Sync(AttackState other) {
            Set(other);
            return Notify();
        }

        public AttackState Start(float when) {
            AttackId++;
            Phase = AttackPhase.Starting;
            
            TargetId = 0;
            WhenHit = 0f;
            Damage = 0;
            HitStyle = HitStyle.None;
            PauseTime = 0f;
            Contact = Vector3.zero;
            Recoil = Vector3.zero;
            
            LastUpdated = when;
            return Notify(when);
        }

        public AttackState Activate(float when) {
            if (Phase == AttackPhase.Starting) {
                Phase = AttackPhase.Active;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public AttackState Reactivate(float when) {
            if (Phase == AttackPhase.Active) {
                AttackId++;
                LastUpdated = when;
                return Activate(when);
            }
            return this;
        }

        public AttackState Recover(float when) {
            if (Phase == AttackPhase.Active) {
                Phase = AttackPhase.Recover;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public AttackState Finish(float when) {
            Phase = AttackPhase.Neutral;
            LastUpdated = when;
            return Notify(when);
        }
        
        public AttackState Connect(
            int targetId,
            int damage,
            HitStyle style,
            float pause,
            Vector3 contact,
            Vector3 knockback,
            Vector3 recoil,
            float when
        ) {
            TargetId = targetId;
            WhenHit = when;
            Damage = damage;
            HitStyle = style;
            PauseTime = pause;
            Contact = contact;
            Recoil = recoil;
            LastUpdated = when;
            return Notify(when);
        }
    }
}