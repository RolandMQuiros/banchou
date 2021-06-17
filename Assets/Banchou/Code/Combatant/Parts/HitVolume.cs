using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Banchou.Pawn;

namespace Banchou.Combatant {
    public class HitVolume : MonoBehaviour {
        public void Construct(
            PawnState pawn
        ) {

        }

        private void OnTriggerEnter(Collider other) {
            var hurtVolume = other.GetComponent<HurtVolume>();
            if (hurtVolume != null) {

            }
        }
    }
}