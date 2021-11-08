using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou {
    [MessagePackObject]
    public class HitState : Notifiable<HitState> {
        [Key(0)][field: SerializeField] public int AttackerId { get; private set; } = default;
        [Key(1)] [field: SerializeField] public KnockbackState Knockback { get; private set; } = new KnockbackState();
    }
    
    [MessagePackObject]
    public class KnockbackState : Notifiable<KnockbackState> {
        [Key(0)][field: SerializeField] public Vector3 Force { get; private set; }
        [Key(1)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializationConstructor]
        public KnockbackState(Vector3 force, float lastUpdated) {
            Force = force;
            LastUpdated = lastUpdated;
        }
        public KnockbackState() { }

        public KnockbackState Hit(Vector3 force, float when) {
            Force = force;
            LastUpdated = when;
            return Notify();
        }
    }
}