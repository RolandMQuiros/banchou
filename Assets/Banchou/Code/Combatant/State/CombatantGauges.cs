using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject]
    public class CombatantGauges : Notifiable<CombatantGauges> {
        [Key(0)][field: SerializeField] public int Health { get; private set; } = 0;
        [Key(1)][field: SerializeField] public Vector3 Knockback { get; private set; } = Vector3.zero;

        [SerializationConstructor]
        public CombatantGauges(int health, Vector3 knockback) {
            Health = health;
            Knockback = knockback;
        }

        public CombatantGauges(int health) {
            Health = health;
        }

        public CombatantGauges Set(int health, Vector3 knockback) {
            Health = health;
            Knockback = knockback;
            Notify();
            return this;
        }

        public CombatantGauges SetHealth(int health) {
            Health = health;
            Notify();
            return this;
        }

        public CombatantGauges SetKnockback(Vector3 knockback) {
            Knockback = knockback;
            Notify();
            return this;
        }
    }
}