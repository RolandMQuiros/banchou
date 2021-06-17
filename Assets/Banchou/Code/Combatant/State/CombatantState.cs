using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantState : Notifiable<CombatantState> {
        [Key(0)][field: SerializeField]
        public CombatantStats Stats { get; private set; }

        [Key(1)][field: SerializeField]
        public CombatantGauges Gauges { get; private set; }

        [Key(2)][field: SerializeField]
        public CombatantAttackState Attack { get; private set; }

        [Key(3)][field: SerializeField]
        public CombatantDefenseState Defense { get; private set; }


        [SerializationConstructor]
        public CombatantState(
            CombatantStats stats,
            CombatantGauges gauges,
            CombatantAttackState attack
        ) {
            Stats = stats;
            Gauges = gauges;
            Attack = attack;
        }

        public CombatantState(CombatantStats stats) {
            Stats = stats;
            Gauges = new CombatantGauges(Stats.MaxHealth);
        }
    }
}