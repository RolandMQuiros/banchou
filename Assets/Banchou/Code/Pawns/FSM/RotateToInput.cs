using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class RotateToInput : FSMBehaviour {
        [SerializeField, Tooltip("How quickly, in degrees per second, the Object will rotate to face its motion vector")]
        private float _rotationSpeed = 1000f;
        [SerializeField, Tooltip("If enabled, immediately snaps rotation to input on state exit")]
        private bool _snapOnExit = true;

        [SerializeField, Tooltip("How long, in seconds, the Object will face a direction before it rotates towards its motion vector")]
        private float _flipDelay = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("When, in normalized state time, the Object will start rotating to input")]
        private float _startTime = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("When, in normalized state time, the Object will stop rotating to input")]
        private float _endTime = 1f;

        public void Construct(
            GameState state,
            PawnState pawn,
            Animator animator,
            Rigidbody rigidbody,
            GetTime getTime,
            GetDeltaTime getDeltaTime
        ) {
            // The object's final facing unit vector angle
            var faceDirection = Vector3.zero;
            var flipTimer = 0f;

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
                    if (direction != Vector3.zero) {
                        // If the movement direction is different enough from the facing direction,
                        // remain facing in the current direction for a short time. Allows the player to
                        // more easily execute Pull Attacks
                        var faceMotionDot = Vector3.Dot(direction, faceDirection);
                        if (faceMotionDot <= -0.01f && flipTimer < _flipDelay) {
                            flipTimer += getDeltaTime();
                        } else {
                            faceDirection = direction.normalized;
                            flipTimer = 0f;
                        }
                    }

                    if (faceDirection != Vector3.zero) {
                        pawn.Spatial.Rotate(
                            Vector3.RotateTowards(
                                rigidbody.transform.forward,
                                faceDirection,
                                _rotationSpeed * getDeltaTime(),
                                0f
                            ),
                            getTime()
                        );
                    }
                })
                .AddTo(this);

            if (_snapOnExit) {
                ObserveStateExit
                    .Subscribe(_ => {
                        // Snap to the facing direction on state exit.
                        // Helps face the character in the intended direction when jumping mid-turn.
                        pawn.Spatial.Rotate(faceDirection, getTime());
                    })
                    .AddTo(this);
            }
        }
    }
}