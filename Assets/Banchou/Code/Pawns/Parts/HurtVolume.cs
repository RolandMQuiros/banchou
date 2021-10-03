using UnityEngine;
using Banchou.Combatant;

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        [SerializeField] private float _damageScale = 1f;
        [SerializeField] private float _knockbackScale = 1f;
        [SerializeField] private float _hitStunScale = 1f;

        private GameState _state;
        private int _pawnId;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _pawnId = getPawnId();
        }
        
        private void OnTriggerEnter(Collider other) {
            var hitVolume = other.GetComponent<HitVolume>();
            if (hitVolume != null && hitVolume.PawnId != _pawnId) {
                _state.HitCombatant(
                    hitVolume.PawnId,
                    _pawnId,
                    hitVolume.Knockback * _knockbackScale,
                    hitVolume.HitStun * _hitStunScale,
                    Mathf.RoundToInt(hitVolume.Damage * _damageScale)
                );
            }
        }
    }
}