using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantGauges : Notifiable<CombatantGauges> {
        [Key(0)][field: SerializeField] public int Health { get; private set; } = 0;
        [Key(1)][field: SerializeField] public float LastUpdated { get; private set; } = 0f;

        [SerializationConstructor]
        public CombatantGauges(int health, float lastUpdated) {
            Health = health;
            LastUpdated = lastUpdated;
        }

        public CombatantGauges(int health) {
            Health = health;
        }

        public CombatantGauges Set(int health, float when) {
            Health = health;
            LastUpdated = when;
            return Notify();
        }

        public CombatantGauges SetHealth(int health, float when) {
            Health = health;
            LastUpdated = when;
            return Notify();
        }

        public CombatantGauges Damage(int damage, float when) {
            Health = Mathf.Max(0, Health - damage);
            LastUpdated = when;
            return Notify();
        }
    }
}