using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantStates : Notifiable<CombatantStates> {
        public event Action<CombatantState> Added;
        public event Action<CombatantState> Removed;


        [Key(0)][field:SerializeField]
        public Dictionary<int, CombatantState> Members { get; private set; } = new Dictionary<int, CombatantState>();

        [SerializationConstructor]
        public CombatantStates(Dictionary<int, CombatantState> members) {
            Members = members;
        }
    }
}