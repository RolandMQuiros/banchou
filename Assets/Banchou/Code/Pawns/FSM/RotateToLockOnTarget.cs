using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToLockOnTarget : FSMBehaviour {
        [SerializeField, Tooltip("Whether or not to ignore rotation speed and instantaneously snap to target rotation")]
        private bool _snap;
        
        [SerializeField, Tooltip("How quickly, in degrees per second, the Pawn will rotate to face its target")]
        private float _rotationSpeed = 1000f;

        private GameState _state;
        private PawnSpatial _spatial;
        private PawnSpatial _targetSpatial;
        private float _deltaTime;

        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            _state = state;
            
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

            state.ObservePawnDeltaTime(pawnId)
                .CatchIgnoreLog()
                .Subscribe(dt => _deltaTime = dt)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_spatial == null || _targetSpatial == null) return;

            var targetDirection = _spatial.DirectionTo(_targetSpatial.Position);
            if (!_snap) {
                targetDirection = Vector3.RotateTowards(
                    _spatial.Forward,
                    targetDirection,
                    Mathf.Deg2Rad * _rotationSpeed * _deltaTime,
                    0f
                );
            }
            _spatial.Rotate(targetDirection, _state.GetTime());
        }
    }
}