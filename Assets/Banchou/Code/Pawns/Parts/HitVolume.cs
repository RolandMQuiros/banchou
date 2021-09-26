using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HitVolume : MonoBehaviour {
        [SerializeField] private int _damage;
        [SerializeField] private float _hitStun;
        [SerializeField] private float _pauseTime;
        [SerializeField] private float _knockback;

        private GameState _state;
        private int _pawnId;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _pawnId = getPawnId();
        }
        
        private void OnTriggerEnter(Collider other) {
            var hitVolume = other.GetComponent<HurtVolume>();
            if (hitVolume != null && hitVolume.PawnId != _pawnId) {
                _state.HitCombatant(
                    _pawnId,
                    hitVolume.PawnId,
                    _knockback * transform.forward,
                    _hitStun,
                    _damage
                );
            }
        }
    }
}