using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HitVolume : MonoBehaviour {
        public int PawnId { get; private set; }
        [field: SerializeField] public float DamageScale { get; private set; } = 1f;
        [field: SerializeField] public float KnockbackScale { get; private set; } = 1f;
        [field: SerializeField] public float HitStunScale { get; private set; }  = 1f;

        private GameState _state;
        private HurtVolume _hurtVolume;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            PawnId = getPawnId();
        }

        private void OnTriggerEnter(Collider other) {
            var hurtVolume = other.GetComponent<HurtVolume>();
            if (hurtVolume != null && hurtVolume.PawnId != PawnId) {
                _hurtVolume = hurtVolume;
            }
        }

        private void LateUpdate() {
            if (_hurtVolume != null) {
                _state.HitCombatant(
                    _hurtVolume.PawnId,
                    PawnId,
                    _hurtVolume.Knockback * KnockbackScale,
                    _hurtVolume.HitStun * HitStunScale,
                    Mathf.RoundToInt(_hurtVolume.Damage * DamageScale)
                );
                _hurtVolume = null;
            }
        }
    }
}