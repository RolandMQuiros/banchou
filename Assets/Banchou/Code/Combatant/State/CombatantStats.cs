using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record CombatantStats(
        CombatantTeam Team = CombatantTeam.Neutral,
        int MaxHealth = 0,
        float Weight = 0f
    ) : NotifiableWithHistory<CombatantStats>(4) {
        [Key(0)][field: SerializeField] public CombatantTeam Team { get; private set; } = Team;
        [Key(1)][field: SerializeField] public int MaxHealth { get; private set; } = MaxHealth;
        [Key(2)][field: SerializeField] public float Weight { get; private set; } = Weight;
        
        public override void Set(CombatantStats other) {
            Team = other.Team;
            MaxHealth = other.MaxHealth;
            Weight = other.Weight;
        }

        public CombatantStats Set(CombatantTeam team, int maxHealth, float weight, float when) {
            var changed = false;
            
            if (Team != team) {
                Team = team;
                changed = true;
            }

            if (MaxHealth != maxHealth) {
                MaxHealth = maxHealth;
                changed = true;
            }

            if (!Mathf.Approximately(Weight, weight)) {
                Weight = weight;
                changed = true;
            }

            return changed ? Notify(when) : this;
        }
    }
}