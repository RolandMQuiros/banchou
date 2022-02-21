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
        bool Blocked = false,
        int Damage = 0,
        float PauseTime = 0f,
        float StunTime = 0f,
        Vector3 Contact = new(),
        Vector3 Knockback = new(),
        Vector3 Recoil = new(),
        bool IsGrab = false,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<AttackState>(32) {
        [field: SerializeField] public int AttackerId { get; } = AttackerId;
        [field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [field: SerializeField] public AttackPhase Phase { get; private set; } = Phase; 
        [field: SerializeField] public int TargetId { get; private set; } = TargetId;
        [field: SerializeField] public bool Blocked { get; private set; } = Blocked;
        [field: SerializeField] public int Damage { get; private set; } = Damage;
        [field: SerializeField] public float PauseTime { get; private set; } = PauseTime;
        [field: SerializeField] public float StunTime { get; private set; } = StunTime;
        [field: SerializeField] public Vector3 Contact { get; private set; } = Contact;
        [field: SerializeField] public Vector3 Knockback { get; private set; } = Knockback;
        [field: SerializeField] public Vector3 Recoil { get; private set; } = Recoil;
        [field: SerializeField] public bool IsGrab { get; private set; } = IsGrab;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public bool Confirmed => TargetId != default && !Blocked;
        
        public float StunTimeAt(float when) => StunTime - (when - LastUpdated + PauseTime);
        
        public float NormalizedStunTimeAt(float when) => Mathf.Approximately(StunTime, 0f) ? 1f :
            Mathf.Clamp01((when - (LastUpdated + PauseTime)) / StunTime);

        public float PauseTimeAt(float when) => PauseTime - (when - LastUpdated);
        
        public float NormalizedPauseTimeAt(float when) => Mathf.Approximately(PauseTime, 0f) ? 1f :
            Mathf.Clamp01((when - LastUpdated) / PauseTime);
        
        public bool IsStunned(float when) => StunTimeAt(when) > 0f;

        public override void Set(AttackState other) {
            AttackId = other.AttackId;
            Phase = other.Phase;
            TargetId = other.TargetId;
            Blocked = other.Blocked;
            Damage = other.Damage;
            PauseTime = other.PauseTime;
            StunTime = other.StunTime;
            Contact = other.Contact;
            Knockback = other.Knockback;
            Recoil = other.Recoil;
            IsGrab = other.IsGrab;
            LastUpdated = other.LastUpdated;
        }

        public AttackState Start(float when) {
            AttackId++;
            Phase = AttackPhase.Starting;
            Blocked = false;
            TargetId = 0;
            Damage = 0;
            PauseTime = 0f;
            StunTime = 0f;
            Contact = Vector3.zero;
            Knockback = Vector3.zero;
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
                TargetId = 0;
                Blocked = false;
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
            Blocked = false;
            TargetId = default;
            LastUpdated = when;
            return Notify(when);
        }
        
        public AttackState Connect(
            int targetId,
            int damage,
            bool blocked,
            float pause,
            float stun,
            Vector3 contact,
            Vector3 knockback,
            Vector3 recoil,
            bool isGrab,
            float when
        ) {
            TargetId = targetId;
            Damage = damage;
            Blocked = blocked;
            PauseTime = pause;
            StunTime = stun;
            Contact = contact;
            Knockback = knockback;
            Recoil = recoil;
            IsGrab = isGrab;
            LastUpdated = when;
            return Notify(when);
        }
    }
}