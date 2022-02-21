using System;
using Banchou.Combatant;
using UnityEngine;
using UniRx;

using Banchou.Player;
using UnityEngine.InputSystem.Utilities;

namespace Banchou.Pawn.FSM {
    public class InputMovement : FSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, OnUpdate = 2 }
        
        [Header("Movement")]
        [SerializeField, Tooltip("How quickly, in units per second, the object moves along its motion vector")]
        private float _movementSpeed = 8f;

        [SerializeField,
         Tooltip("How quickly, in units per second per second, the object accelerates to Movement Speed")]
        private float _acceleration = 1000f;

        [SerializeField] private ApplyEvent _readEvent = ApplyEvent.OnUpdate;
        
        [Header("Lock On Approaches")]
        
        [SerializeField,
         Tooltip("Whether or not to use a different movement speed when moving towards a lock-on target")]
        private bool _handleApproaches = false;

        [SerializeField, Tooltip("How quickly, in units per second, the Pawn moves toward a lock on target")]
        private float _approachSpeed = 8f;
        
        [SerializeField,
         Tooltip("Angle centered on Pawn's forward vector, within which a movement is considered an approach")]
        private float _approachAngle = 30f;
        
        [SerializeField,
         Tooltip("Whether or not to snap all movements within the approach angle towards the lock-on target")]
        private bool _snapToApproach = false;
        
        [Header("Animation")]
        [SerializeField] private float _animationAcceleration = 1f;

        [Header("Output")]
        [SerializeField, Tooltip("Animation parameter to write movement speed")]
        private string _movementSpeedOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write lateral movement speed")]
        private string _velocityRightOut = string.Empty;
        [SerializeField, Tooltip("Animation parameter to write forward movement speed")]
        private string _velocityForwardOut = string.Empty;

        [SerializeField, Tooltip("Normalize the animation parameters")]
        private bool _normalizeOutput = true;
        
        [SerializeField, Tooltip("True to clear all parameters to zero on state entry")]
        private bool _clearOutOnEntry = true;
        
        [SerializeField, Tooltip("True to clear all parameters to zero on state exit")]
        private bool _clearOutOnExit = true;

        private GameState _state;
        private PlayerInputState _input;
        private PawnSpatial _spatial;
        private float _speed;
        private Vector3 _velocity;

        private PawnSpatial _targetSpatial;
        private float _approachDot;

        private float _speedOut = 0f;
        private float _forwardSpeedOut = 0f;
        private float _rightSpeedOut = 0f;

        private int _speedHash = 0;
        private int _rightSpeedHash = 0;
        private int _forwardSpeedHash = 0;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            var pawnId = getPawnId();
            
            _state.ObservePawnSpatialChanges(pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObservePawnInput(pawnId)
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);

            if (_handleApproaches) {
                _state.ObserveLockOn(pawnId)
                    .SelectMany(targetId => _state.ObservePawnSpatial(targetId))
                    .CatchIgnoreLog()
                    .Subscribe(targetSpatial => _targetSpatial = targetSpatial)
                    .AddTo(this);
            }

            _speedHash = Animator.StringToHash(_movementSpeedOut);
            _rightSpeedHash = Animator.StringToHash(_velocityRightOut);
            _forwardSpeedHash = Animator.StringToHash(_velocityForwardOut);
            
            _approachDot = Mathf.Cos(Mathf.Deg2Rad * _approachAngle / 2f);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            if (_readEvent.HasFlag(ApplyEvent.OnEnter)) {
                _velocity = _movementSpeed * _input.Direction;
            }

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
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_input == null) return;
            
            var dt = _state.GetDeltaTime();

            if (_readEvent.HasFlag(ApplyEvent.OnUpdate)) {
                var speed = _movementSpeed;
                var direction = _input.Direction;
                
                // If there's a lock-on target, check if we're moving towards it
                if (_handleApproaches && _targetSpatial != null) {
                    var lockOnDirection = _spatial.DirectionTo(_targetSpatial.Position);
                    var dot = Vector3.Dot(lockOnDirection, _input.Direction);
                    if (dot >= _approachDot) {
                        speed = _approachSpeed;
                        if (_snapToApproach) {
                            direction = lockOnDirection;
                        }
                    }
                }
                
                // Calculate new velocity
                _velocity = Vector3.MoveTowards(_velocity, speed * direction, _acceleration * dt * dt);
            }

            var offset = _velocity * _state.GetDeltaTime();
            _spatial.Move(offset, _state.GetTime());

            // Write to output variables
            if (!string.IsNullOrWhiteSpace(_movementSpeedOut)) {
                if (_speedHash != 0) {
                    _speedOut = Mathf.MoveTowards(_speedOut, _velocity.magnitude, _animationAcceleration);
                    animator.SetFloat(_speedHash, _normalizeOutput ? _speedOut / _movementSpeed : _speedOut);
                }

                if (_rightSpeedHash != 0) {
                    _rightSpeedOut = Mathf.MoveTowards(_rightSpeedOut, Vector3.Dot(_velocity, _spatial.Right), _animationAcceleration);
                    animator.SetFloat(_rightSpeedHash, _normalizeOutput ? _rightSpeedOut / _movementSpeed : _rightSpeedOut);
                }

                if (_forwardSpeedHash != 0) {
                    _forwardSpeedOut = Mathf.MoveTowards(_forwardSpeedOut, Vector3.Dot(_velocity, _spatial.Forward), _animationAcceleration);
                    animator.SetFloat(_forwardSpeedHash, _normalizeOutput ? _forwardSpeedOut / _movementSpeed : _forwardSpeedOut);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
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