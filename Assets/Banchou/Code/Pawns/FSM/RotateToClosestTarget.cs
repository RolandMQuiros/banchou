using System.Linq;
using Banchou.Combatant;
using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToClosestTarget : FSMBehaviour {
        [SerializeField] private float _scanRange = 3f;
        [SerializeField] private float _scanAngle = 30f;
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
            if (_combatant.LockOnTarget == default) {
                _target = _state.GetCombatantSpatials()
                    .Where(target => target.PawnId != _pawnId)
                    .Select(target => (
                        Target: target,
                        Offset: _spatial.DirectionTo(target.Position)
                    ))
                    .Where(args => Vector3.Dot(args.Offset.normalized, _spatial.Forward) >= _scanDot &&
                                   args.Offset.magnitude <= _scanRange)
                    .OrderBy(args => Vector3.Dot(args.Offset, _spatial.Forward))
                    .Select(args => args.Target)
                    .FirstOrDefault();
            } else {
                _target = _state.GetPawnSpatial(_combatant.LockOnTarget);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var stateTime = stateInfo.normalizedTime;
            if (_target != null && stateTime >= _fromStateTime && stateTime <= _toStateTime) {
                var direction = _spatial.DirectionTo(_target.Position);
                var forward = _spatial.Forward;
                if (_input != null && _input.Direction != Vector3.zero) {
                    forward = _input.Direction.normalized;
                }
                
                _spatial.Rotate(
                    Vector3.RotateTowards(forward, direction, _rotationSpeed, 0f),
                    _state.GetTime()
                );
            }
        }
    }
}