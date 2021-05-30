using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class RotateToInput : FSMBehaviour {
        [SerializeField, Tooltip("How quickly, in degrees per second, the Object will rotate to face its motion vector")]
        private float _rotationSpeed = 1000f;

        [SerializeField, Tooltip("Whether or not to ignore rotation speed and instantaneously snap to target rotation")]
        private bool _snap = false;

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

        public void Construct(
            GameState state,
            PawnState pawn,
            Animator animator,
            Rigidbody rigidbody
        ) {
            // The object's final facing unit vector angle
            var faceDirection = Vector3.zero;
            var flipTimer = 0f;
            var flipMagnitudeThreshold = _flipMagnitudeThreshold * _flipDirectionThreshold;

            ObserveStateEnter
                .Subscribe(_ => {
                    faceDirection = pawn.Spatial.Forward;
                    flipTimer = 0f;
                })
                .AddTo(this);

            ObserveStateUpdate
                .Select(stateUnit => stateUnit.StateInfo.normalizedTime % 1)
                .Where(time => time >= _startTime && time <= _endTime)
                .WithLatestFrom(state.ObservePawnInput(pawn.PawnId).Select(input => input.Direction), (_, input) => input)
                .Subscribe(direction => {
                    if (direction.sqrMagnitude > flipMagnitudeThreshold) {
                        // If the movement direction is different enough from the facing direction,
                        // remain facing in the current direction for a short time. Allows the player to
                        // more easily execute Pull Attacks
                        var faceMotionDot = Vector3.Dot(direction, faceDirection);
                        if (faceMotionDot <= _flipDirectionThreshold && flipTimer < _flipDelay) {
                            flipTimer += state.GetDeltaTime();
                        } else {
                            faceDirection = direction.normalized;
                            flipTimer = 0f;
                        }
                    } else {
                        flipTimer = 0f;
                    }

                    if (faceDirection != Vector3.zero) {
                        if (_snap) {
                            pawn.Spatial.Rotate(faceDirection, state.GetTime());
                        } else {
                            pawn.Spatial.Rotate(
                                Vector3.RotateTowards(
                                    rigidbody.transform.forward,
                                    faceDirection,
                                    _rotationSpeed * state.GetDeltaTime(),
                                    0f
                                ),
                                state.GetTime()
                            );
                        }
                    }
                })
                .AddTo(this);

            if (_snapOnExit) {
                ObserveStateExit
                    .Subscribe(_ => {
                        // Snap to the facing direction on state exit.
                        // Helps face the character in the intended direction when jumping mid-turn.
                        pawn.Spatial.Rotate(faceDirection, state.GetTime());
                    })
                    .AddTo(this);
            }
        }
    }
}