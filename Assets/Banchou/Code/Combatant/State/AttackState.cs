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
    
    [Serializable, MessagePackObject]
    public record AttackState(
        int AttackId = 0,
        AttackPhase Phase = AttackPhase.Neutral,
        int TargetId = 0,
        int Damage = 0,
        float PauseTime = 0f,
        Vector3 Contact = new(),
        Vector3 Recoil = new(),
        float LastUpdated = 0f
    ) : NotifiableWithHistory<AttackState>(32) {
        [field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [field: SerializeField] public AttackPhase Phase { get; private set; } = Phase; 
        [field: SerializeField] public int TargetId { get; private set; } = TargetId;
        [field: SerializeField] public int Damage { get; private set; } = Damage;
        [field: SerializeField] public float PauseTime { get; private set; } = PauseTime;
        [field: SerializeField] public Vector3 Contact { get; private set; } = Contact;
        [field: SerializeField] public Vector3 Recoil { get; private set; } = Recoil;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public bool IsConfirmed => TargetId != default;
        
        public float NormalizedPauseTimeAt(float when) => Mathf.Approximately(PauseTime, 0f) ? 1f :
            Mathf.Clamp01((when - LastUpdated) / PauseTime);

        public override void Set(AttackState other) {
            AttackId = other.AttackId;
            TargetId = other.TargetId;
            Phase = other.Phase;
            Damage = other.Damage;
            PauseTime = other.PauseTime;
            Recoil = other.Recoil;
            LastUpdated = other.LastUpdated;
        }

        public AttackState Start(float when) {
            AttackId++;
            Phase = AttackPhase.Starting;
            TargetId = 0;
            Damage = 0;
            PauseTime = 0f;
            Recoil = Vector3.zero;
            return UpdateTimes(when);
        }

        public AttackState Activate(float when) {
            if (Phase == AttackPhase.Starting) {
                Phase = AttackPhase.Active;
                return UpdateTimes(when);
            }
            return this;
        }

        public AttackState Reactivate(float when) {
            if (Phase == AttackPhase.Active) {
                AttackId++;
                return Activate(when);
            }
            return this;
        }

        public AttackState Recover(float when) {
            if (Phase == AttackPhase.Active) {
                Phase = AttackPhase.Recover;
                return UpdateTimes(when);
            }
            return this;
        }

        public AttackState Finish(float when) {
            Phase = AttackPhase.Neutral;
            return UpdateTimes(when);
        }
        
        public AttackState Confirm(int targetId, int damage, float pause, Vector3 contact, Vector3 recoil, float when) {
            TargetId = targetId;
            Damage = damage;
            PauseTime = pause;
            Contact = contact;
            Recoil = recoil;
            return UpdateTimes(when);
        }

        private AttackState UpdateTimes(float when) {
            var diff = when - LastUpdated;
            PauseTime = Mathf.Clamp01(PauseTime - diff);
            LastUpdated = when;
            return Notify(when);
        }
    }
}