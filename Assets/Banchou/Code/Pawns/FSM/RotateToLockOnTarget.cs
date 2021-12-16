using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToLockOnTarget : FSMBehaviour {
        [SerializeField, Tooltip("How quickly, in degrees per second, the Pawn will rotate to face its target")]
        private float _rotationSpeed = 1000f;

        private GameState _state;
        private PawnSpatial _spatial;
        private PawnSpatial _targetSpatial;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            var pawnId = getPawnId();

            state.ObservePawnSpatial(pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            
            state.ObserveCombatantChanges(pawnId)
                .Select(combatant => combatant.LockOnTarget)
                .DistinctUntilChanged()
                .CatchIgnoreLog()
                .Subscribe(targetId => {
                    if (targetId == default) {
                        _targetSpatial = null;
                    } else {
                        _targetSpatial = state.GetPawnSpatial(targetId);
                    }
                })
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_spatial == null || _targetSpatial == null) return;
            
            var targetDirection = (_targetSpatial.Position - _spatial.Position).normalized;
            _spatial.Rotate(
                Vector3.RotateTowards(
                    _spatial.Forward,
                    targetDirection,
                    _rotationSpeed * _state.GetDeltaTime(),
                    0f
                ),
                _state.GetTime()
            );
        }
    }
}