using UnityEngine;
using UniRx;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class InputMovement : FSMBehaviour {
        [Header("Movement")]
        [SerializeField, Tooltip("How quickly, in units per second, the object moves along its motion vector")]
        private float _movementSpeed = 8f;

        [SerializeField,
         Tooltip("How quickly, in units per second per second, the object accelerates to Movement Speed")]
        private float _acceleration = 1000f;

        [Header("Animation")]
        [SerializeField] private float _animationAcceleration = 1f;

        [Header("Output")]
        [SerializeField, Tooltip("Animation parameter to write movement speed")]
        private string _movementSpeedOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write lateral movement speed")]
        private string _velocityRightOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write forward movement speed")]
        private string _velocityForwardOut = string.Empty;
        
        [SerializeField, Tooltip("True to clear all parameters to zero on state entry")]
        private bool _clearOutOnEntry = true;
        
        [SerializeField, Tooltip("True to clear all parameters to zero on state exit")]
        private bool _clearOutOnExit = true;

        private GameState _state;
        private PlayerInputState _input;
        private PawnSpatial _spatial;
        private float _speed;
        private Vector3 _velocity;

        private float _speedOut = 0f;
        private float _forwardSpeedOut = 0f;
        private float _rightSpeedOut = 0f;

        private int _speedHash = 0;
        private int _rightSpeedHash = 0;
        private int _forwardSpeedHash = 0;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObservePawnInput(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);

            _speedHash = Animator.StringToHash(_movementSpeedOut);
            _rightSpeedHash = Animator.StringToHash(_velocityRightOut);
            _forwardSpeedHash = Animator.StringToHash(_velocityForwardOut);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_clearOutOnEntry) {
                _speedOut = 0f;
                _rightSpeedOut = 0f;
                _forwardSpeedOut = 0f;
                if (_speedHash != 0) animator.SetFloat(_speedHash, 0f);
                if (_rightSpeedHash != 0) animator.SetFloat(_rightSpeedHash, 0f);
                if (_forwardSpeedHash != 0) animator.SetFloat(_forwardSpeedHash, 0f);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_input == null) return;
            
            var dt = _state.GetDeltaTime();
            _velocity = Vector3.MoveTowards(
                _velocity,
                _movementSpeed * _input.Direction,
                _acceleration * dt * dt
            );
            var offset = _velocity * _state.GetDeltaTime();
            _spatial.Move(offset, _state.GetTime());

            // Write to output variables
            if (!string.IsNullOrWhiteSpace(_movementSpeedOut)) {
                if (_speedHash != 0) {
                    _speedOut = Mathf.MoveTowards(_speedOut, _velocity.magnitude, _animationAcceleration);
                    animator.SetFloat(_speedHash, _speedOut);
                }

                if (_rightSpeedHash != 0) {
                    _rightSpeedOut = Mathf.MoveTowards(_rightSpeedOut, Vector3.Dot(_velocity, _spatial.Right), _animationAcceleration);
                    animator.SetFloat(_rightSpeedHash, _rightSpeedOut);
                }

                if (_forwardSpeedHash != 0) {
                    _forwardSpeedOut = Mathf.MoveTowards(_forwardSpeedOut, Vector3.Dot(_velocity, _spatial.Forward), _animationAcceleration);
                    animator.SetFloat(_forwardSpeedHash, _forwardSpeedOut);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_clearOutOnExit) {
                _speedOut = 0f;
                _rightSpeedOut = 0f;
                _forwardSpeedOut = 0f;
                if (_speedHash != 0) animator.SetFloat(_speedHash, 0f);
                if (_rightSpeedHash != 0) animator.SetFloat(_rightSpeedHash, 0f);
                if (_forwardSpeedHash != 0) animator.SetFloat(_forwardSpeedHash, 0f);
            }
            _velocity = Vector3.zero;
        }
    }
}