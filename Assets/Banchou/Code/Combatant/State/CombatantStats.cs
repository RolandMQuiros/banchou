using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantStats : Notifiable<CombatantStats> {
        [Key(0)][field: SerializeField] public int MaxHealth { get; private set; } = 0;
        [Key(1)][field: SerializeField] public float Weight { get; private set; } = 0f;

        [SerializationConstructor]
        public CombatantStats(
            int maxHealth,
            float weight
        ) {
            MaxHealth = maxHealth;
            Weight = weight;
        }
    }
}