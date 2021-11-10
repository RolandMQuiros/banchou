using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public record CombatantStats(
        int MaxHealth = 0,
        float Weight = 0f
    ) : NotifiableWithHistory<CombatantStats>(4) {
        [field: SerializeField] public int MaxHealth { get; private set; } = MaxHealth;
        [field: SerializeField] public float Weight { get; private set; } = Weight;
        
        public override void Set(CombatantStats other) {
            MaxHealth = other.MaxHealth;
            Weight = other.Weight;
        }
    }
}