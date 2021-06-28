using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantDefenseState : Notifiable<CombatantDefenseState> {
        [Key(0)][field: SerializeField]
        public CombatantBlockDirection BlockDirection { get; private set; } = CombatantBlockDirection.Neutral;

        [Key(1)][field: SerializeField]
        public Vector3 Knockback { get; private set; } = Vector3.zero;

        [Key(2)][field: SerializeField]
        public float StunTimestamp { get; private set; } = 0f;

        [Key(3)][field: SerializeField]
        public float CounteredTimestamp { get; private set; } = 0f;

        [Key(4)][field: SerializeField]
        public float LastUpdated { get; private set; } = 0f;

        public CombatantDefenseState() { }

        [SerializationConstructor]
        public CombatantDefenseState(
            CombatantBlockDirection blockDirection,
            Vector3 knockback,
            float stunTimestamp,
            float counteredTimestamp,
            float lastUpdated
        ) {
            BlockDirection = blockDirection;
            Knockback = knockback;
            StunTimestamp = stunTimestamp;
            CounteredTimestamp = counteredTimestamp;
            LastUpdated = lastUpdated;
        }

        public CombatantDefenseState Block(CombatantBlockDirection directions, float when) {
            BlockDirection = directions;
            LastUpdated = when;
            return Notify();
        }

        public CombatantDefenseState Countered(float when) {
            StunTimestamp = when;
            LastUpdated = when;
            return Notify();
        }

        public CombatantDefenseState Push(Vector3 push, float when) {
            Knockback = push;
            return Notify();
        }
    }

    [Flags]
    public enum CombatantBlockDirection : short {
        Neutral = 0,
        Forward = 1,
        ForwardRight = 1 << 1,
        Right = 1 << 2,
        BackRight = 1 << 3,
        Back = 1 << 4,
        BackLeft = 1 << 5,
        Left = 1 << 6,
        ForwardLeft = 1 << 7,
        Above = 1 << 8
    }
}