using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantDefenseState : Notifiable<CombatantDefenseState> {
        [Key(0)][field: SerializeField]
        public CombatantBlockDirection BlockDirection { get; private set; } = CombatantBlockDirection.Neutral;

        [Key(1)][field: SerializeField]
        public float LastUpdated { get; private set; } = 0f;

        public CombatantDefenseState Block(CombatantBlockDirection directions, float when) {
            BlockDirection = directions;
            LastUpdated = when;
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