using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantStats : NotifiableWithHistory<CombatantStats> {
        [Key(0)][field: SerializeField] public int MaxHealth { get; private set; } = 0;
        [Key(1)][field: SerializeField] public float Weight { get; private set; } = 0f;
        
        public CombatantStats() : base(4) { }
        
        [SerializationConstructor]
        public CombatantStats(
            int maxHealth,
            float weight
        ) : base(4) {
            MaxHealth = maxHealth;
            Weight = weight;
        }

        public override void Set(CombatantStats other) {
            MaxHealth = other.MaxHealth;
            Weight = other.Weight;
        }
    }
}