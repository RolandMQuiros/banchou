using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;
// ReSharper disable EventNeverSubscribedTo.Global

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantStates : Notifiable<CombatantStates> {
        public event Action<CombatantState> Added;
        public event Action<CombatantState> Removed;

        [Key(0)][field:SerializeField]
        public Dictionary<int, CombatantState> Members { get; private set; } = new Dictionary<int, CombatantState>();

        public CombatantStates() { }

        [SerializationConstructor]
        public CombatantStates(Dictionary<int, CombatantState> members) {
            Members = members;
        }

        public CombatantStates SetCombatant(int pawnId, int maxHealth, out CombatantState combatant) {
            combatant = new CombatantState(maxHealth);
            Members[pawnId] = combatant;
            Added?.Invoke(combatant);
            return Notify();
        }
    }
}