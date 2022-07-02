using System;
using Banchou.Combatant;
using UnityEngine;
using UniRx;
using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class InputMovement : PawnFSMBehaviour {
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
        private FloatFSMParameter[] _movementSpeedOut;
        [SerializeField, Tooltip("Animation parameter to write lateral movement speed")]
        private FloatFSMParameter[] _velocityRightOut;
        [SerializeField, Tooltip("Animation parameter to write forward movement speed")]
        private FloatFSMParameter[] _velocityForwardOut;

        private float _approachDot;
        
        private PlayerInputState _input;
        private PawnSpatial _spatial;
        private PawnSpatial _targetSpatial;
        private float _timeScale;
        
        private Vector3 _velocity;
        private float _speedOut = 0f;
        private float _forwardSpeedOut = 0f;
        private float _rightSpeedOut = 0f;


        public void Construct(GameState state, GetPawnId getPawnId) {
            ConstructCommon(state, getPawnId);
            
            State.ObservePawnSpatialChanges(PawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            State.ObservePawnInput(PawnId)
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);

            if (_handleApproaches) {
                State.ObserveLockOn(PawnId)
                    .SelectMany(targetId => State.ObservePawnSpatial(targetId))
                    .CatchIgnoreLog()
                    .Subscribe(targetSpatial => _targetSpatial = targetSpatial)
                    .AddTo(this);
            }
            
            _approachDot = Mathf.Cos(Mathf.Deg2Rad * _approachAngle / 2f);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            if (_readEvent.HasFlag(ApplyEvent.OnEnter)) {
                _velocity = _movementSpeed * _input.Direction;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_input == null) return;
            if (_spatial.IsSync) {
                _velocity = Vector3.zero;
            }

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
                _velocity = Vector3.MoveTowards(_velocity, speed * direction, _acceleration * DeltaTime * DeltaTime);
            }

            var offset = _velocity * DeltaTime;
            _spatial.Move(offset, State.GetTime());

            // Write to output variables
            if (_movementSpeedOut.Length > 0) {
                _speedOut = Mathf.MoveTowards(_speedOut, _velocity.magnitude, _animationAcceleration);
                _movementSpeedOut.ApplyAll(animator, _speedOut);
            }

            if (_velocityRightOut.Length > 0) {
                _rightSpeedOut = Mathf.MoveTowards(_rightSpeedOut, Vector3.Dot(_velocity, _spatial.Right), _animationAcceleration);
                _velocityRightOut.ApplyAll(animator, _rightSpeedOut);
            }

            if (_velocityForwardOut.Length > 0) {
                _forwardSpeedOut = Mathf.MoveTowards(_forwardSpeedOut, Vector3.Dot(_velocity, _spatial.Forward), _animationAcceleration);
                _velocityForwardOut.ApplyAll(animator, _forwardSpeedOut);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            _speedOut = 0f;
            _rightSpeedOut = 0f;
            _forwardSpeedOut = 0f;
            _velocity = Vector3.zero;
        }
    }
}