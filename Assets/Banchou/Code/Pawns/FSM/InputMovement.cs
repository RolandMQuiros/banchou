using UnityEngine;
using UniRx;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class InputMovement : FSMBehaviour {
        [Header("Movement")]
        [SerializeField, Tooltip("How quickly, in units per second, the object moves along its motion vector")]
        private float _movementSpeed = 8f;

        [Header("Animation")]
        [SerializeField] private float _acceleration = 1f;

        [Header("Animation Parameters")]
        [SerializeField, Tooltip("Animation parameter to write movement speed")]
        private string _movementSpeedOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write lateral movement speed")]
        private string _velocityRightOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write forward movement speed")]
        private string _velocityForwardOut = string.Empty;
        [SerializeField, Tooltip("True to clear all parameters to zero on state exit")]
        private bool _clearOutOnExit = true;

        private GameState _state;
        private PlayerInputState _input;
        private PawnSpatial _spatial;
        
        private float _speed = 0f;
        private float _forwardSpeed = 0f;
        private float _rightSpeed = 0f;
        
        private int _speedOut = 0;
        private int _rightSpeedOut = 0;
        private int _forwardSpeedOut = 0;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            _state = state;
            _state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObservePawnInput(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);
            
            _speedOut = Animator.StringToHash(_movementSpeedOut);
            _rightSpeedOut = Animator.StringToHash(_velocityRightOut);
            _forwardSpeedOut = Animator.StringToHash(_velocityForwardOut);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var velocity = _input.Direction * _movementSpeed;
            var offset = velocity * _state.GetDeltaTime();
            _spatial.Move(offset, _state.GetTime());

            // Write to output variables
            if (!string.IsNullOrWhiteSpace(_movementSpeedOut)) {
                if (_speedOut != 0) {
                    _speed = Mathf.MoveTowards(_speed, velocity.magnitude, _acceleration);
                    animator.SetFloat(_speedOut, _speed);
                }

                if (_rightSpeedOut != 0) {
                    _rightSpeed = Mathf.MoveTowards(_rightSpeed, Vector3.Dot(velocity, _spatial.Right), _acceleration);
                    animator.SetFloat(_rightSpeedOut, _rightSpeed);
                }

                if (_forwardSpeedOut != 0) {
                    _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, Vector3.Dot(velocity, _spatial.Forward), _acceleration);
                    animator.SetFloat(_forwardSpeedOut, _forwardSpeed);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_clearOutOnExit) {
                animator.SetFloat(_speedOut, 0f);
                animator.SetFloat(_rightSpeedOut, 0f);
                animator.SetFloat(_forwardSpeedOut, 0f);
            }
        }
    }
}