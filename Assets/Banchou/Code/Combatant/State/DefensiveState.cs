using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class DefensiveState : Notifiable<CombatantState> {
        [Key(1)][field: SerializeField] public bool IsInvincible { get; private set; }
        [Key(2)][field: SerializeField] public CombatantAttackPhase AttackPhase { get; private set; }
        [Key(3)][field: SerializeField] public float GuardTime { get; private set; }
    }
    
    public enum CombatantAttackPhase {
        Neutral,
        Starting,
        Active,
        Recover
    }
}