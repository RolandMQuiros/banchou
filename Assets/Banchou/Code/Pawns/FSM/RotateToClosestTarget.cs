using System.Linq;
using Banchou.Combatant;
using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToClosestTarget : FSMBehaviour {
        [SerializeField, Tooltip("Only rotate to lock-on target, if one exists")]
        private bool _onlyLockOnTarget;

        [SerializeField, Tooltip("Use player input direction to search")]
        private bool _useInputDirection = true;

        [SerializeField] private float _scanRange = 3f;
        [SerializeField] private float _scanAngle = 30f;
        [SerializeField, Tooltip("Whether or not to ignore rotation speed and instantaneously snap to target rotation")]
        private bool _snap;
        [SerializeField] private float _rotationSpeed = 1000f;
        [SerializeField, Min(0f)] private float _fromStateTime = 0f;
        [SerializeField, Min(0f)] private float _toStateTime = 1f;

        private int _pawnId;
        private GameState _state;
        private PlayerInputState _input;
        private PawnSpatial _spatial;
        private CombatantState _combatant;

        private PawnSpatial _target;
        private float _scanDot;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _pawnId = getPawnId();

            state.ObservePawnInput(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);
            state.ObservePawnSpatial(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            state.ObserveCombatant(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
            
            _scanDot = Mathf.Cos(Mathf.Deg2Rad * _scanAngle / 2f);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            var hasInputDirection = _useInputDirection && _input != null && _input.Direction != Vector3.zero;
            var hasLockOnTarget = _combatant.LockOnTarget != default;

            // Only rotate towards the lock-on target if there's no directional input, or we're explicitly locked to
            // only facing the lock-on target
            if (hasLockOnTarget && (!hasInputDirection || _onlyLockOnTarget)) {
                _target = _state.GetPawnSpatial(_combatant.LockOnTarget);
            } else {
                _target = _state.GetCombatantSpatials()
                    .Where(target => target.PawnId != _pawnId)
                    .Select(target => {
                        // Prioritize input direction for search
                        var forward = _spatial.Forward;
                        if (hasInputDirection) {
                            forward = _input.Direction.normalized;
                        }
                        var offset = target.Position - _spatial.Position;
                        return (
                            Target: target,
                            Distance: offset.magnitude,
                            Dot: Vector3.Dot(offset.normalized, forward)
                        );
                    })
                    .Where(args => args.Distance <= _scanRange && args.Dot > _scanDot)
                    .OrderBy(args => (2f - Mathf.Abs(args.Dot)) * args.Distance)
                    .Select(args => args.Target)
                    .FirstOrDefault();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            var stateTime = stateInfo.normalizedTime;
            var withinTimeframe = stateTime >= _fromStateTime && stateTime <= _toStateTime;

            if (_target == null) {
                
            } else if (withinTimeframe) {
                var direction = _spatial.DirectionTo(_target.Position);
                var forward = _spatial.Forward;
                if (_input != null && _input.Direction != Vector3.zero) {
                    forward = _input.Direction.normalized;
                }

                if (_snap) {
                    _spatial.Rotate(direction, _state.GetTime());
                } else {
                    _spatial.Rotate(
                        Vector3.RotateTowards(forward, direction, Mathf.Deg2Rad * _rotationSpeed, 0f),
                        _state.GetTime()
                    );
                }
            }
        }
    }
}