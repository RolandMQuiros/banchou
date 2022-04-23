using UnityEngine;
using UniRx;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class RotateToInput : PawnFSMBehaviour {
        [SerializeField, Tooltip("Whether or not to rotate to the direction opposite to the input direction")]
        private bool _invertDirection;
        
        [SerializeField, Tooltip("How quickly, in degrees per second, the Object will rotate to face its motion vector")]
        private float _rotationSpeed = 1000f;

        [SerializeField, Tooltip("If true, only rotate when a direction is held")]
        private bool _hold;

        [SerializeField, Tooltip("Whether or not to ignore rotation speed and instantaneously snap to target rotation")]
        private bool _snap;

        [SerializeField, Tooltip("If enabled, immediately snaps rotation to input on state exit")]
        private bool _snapOnExit = true;

        #region Flip Timing
        [Header("Flip Timer")]

        [SerializeField, Tooltip("How long, in seconds, the Object will face a direction before it rotates towards the input vector")]
        private float _flipDelay = 0f;

        [SerializeField, Range(0, 1f)]
        [Tooltip("The input direction must have a magnitude larger than this before the flip timer starts counting down")]
        private float _flipMagnitudeThreshold = 0.4f;

        [SerializeField, Range(-1f, 1f)]
        [Tooltip("The difference between the input and current directions before the flip timer starts counting down.")]
        private float _flipDirectionThreshold = 0.01f;
        #endregion

        #region State Timing
        [Header("State Timing")]

        [SerializeField, Range(0f, 1f), Tooltip("When, in normalized state time, the Object will start rotating to input")]
        private float _startTime = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("When, in normalized state time, the Object will stop rotating to input")]
        private float _endTime = 1f;
        #endregion
        
        private PawnSpatial _spatial;
        private PlayerInputState _input;
        private Rigidbody _body;
        
        // The object's final facing unit vector angle
        private Vector3 _faceDirection = Vector3.zero;
        private float _flipTimer;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody body) {
            ConstructCommon(state, getPawnId);
            _body = body;
            State.ObservePawnSpatial(PawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            State.ObservePawnSpatialChanges(PawnId)
                .DistinctUntilChanged(spatial => spatial.IsSync)
                .Subscribe(_ => _faceDirection = _spatial.Forward)
                .AddTo(this);
            State.ObservePawnInput(PawnId)
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            _faceDirection = _spatial.Forward;
            _flipTimer = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_input == null) return;
            
            var stateTime = stateInfo.normalizedTime % 1;
            if (stateTime >= _startTime && stateTime <= _endTime && (!_hold || _input.Direction != Vector3.zero)) {
                var direction = _invertDirection ? -_input.Direction : _input.Direction;
                var flipMagnitudeThreshold = _flipMagnitudeThreshold * _flipDirectionThreshold;

                if (direction.sqrMagnitude > flipMagnitudeThreshold) {
                    // If the movement direction is different enough from the facing direction,
                    // remain facing in the current direction for a short time. Allows the player to
                    // more easily execute Pull Attacks
                    var faceMotionDot = Vector3.Dot(direction, _faceDirection);
                    if (faceMotionDot <= _flipDirectionThreshold && _flipTimer < _flipDelay) {
                        _flipTimer += DeltaTime;
                    }
                    else {
                        _faceDirection = direction.normalized;
                        _flipTimer = 0f;
                    }
                } else {
                    _flipTimer = 0f;
                }

                if (_faceDirection != Vector3.zero) {
                    if (_snap) {
                        _spatial.Rotate(_faceDirection, State.GetTime());
                    } else {
                        _spatial.Rotate(
                            Vector3.RotateTowards(
                                _body.rotation * Vector3.forward,
                                _faceDirection,
                                _rotationSpeed * DeltaTime,
                                0f
                            ),
                            State.GetTime()
                        );
                    }
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_snapOnExit) {
                // Snap to the facing direction on state exit.
                // Helps face the character in the intended direction when jumping mid-turn.
                _spatial.Rotate(_faceDirection, State.GetTime());
            }
        }
    }
}