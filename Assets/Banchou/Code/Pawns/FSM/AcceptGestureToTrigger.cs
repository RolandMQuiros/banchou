using System.Linq;
using UnityEngine;
using UniRx;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    /*
        Maybe this should be moved/replicated on MonoBehaviour level, so we at least have them all in one
        place to generate command lists from. Then AIs will just need to force trigger names directly instead
        of input commands?

        but also AIs inputting commands kinda rules.
    */
    public class AcceptGestureToTrigger : FSMBehaviour {
        [SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
        private InputCommand[] _inputSequence = null;
        
        [SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
        public float _inputLifetime = 0.1666667f; // Approximately 10 frames
        
        [SerializeField, Tooltip("A command gesture asset. Overrides the Input Sequence and Lifetime if provided.")]
        private PlayerCommandGesture _overrideGesture = null;
        
        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is accepted")]
        private float _acceptFromStateTime = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is no longer accepted")]
        private float _acceptUntilStateTime = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the triggers are set, if the gesture was accepted")]
        private float _bufferUntilStateTime = 0f;
        
        [SerializeField, Tooltip("The name of the output trigger parameters to set if the gesture was input correctly")]
        private string[] _outputParameters = null;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            if (_overrideGesture != null) {
                _inputSequence = _overrideGesture.Sequence;
                _inputLifetime = _overrideGesture.Lifetime;
            }
            
            if (_inputSequence.Length == 0) return;

            var outputHashes = _outputParameters.Select(Animator.StringToHash);
            var commandMask = _inputSequence.Aggregate((prev, next) => prev | next);

            var gestureInput = state.ObservePawnInputCommands(getPawnId())
                .Where(unit => IsStateActive && (unit.Command & commandMask) != InputCommand.None)
                .StartWith((Command: InputCommand.Neutral, When: state.GetTime()))
                .Pairwise()
                .Scan(0, (sequenceIndex, unitPair) => {
                    if (sequenceIndex >= _inputSequence.Length) {
                        sequenceIndex = 0;
                    }

                    var sequenceStarted = sequenceIndex > 0;
                    var previousCommandTooOld = unitPair.Current.When - unitPair.Previous.When >= _inputLifetime;

                    if (sequenceStarted && previousCommandTooOld) {
                        return 0;
                    }

                    if ((unitPair.Current.Command & _inputSequence[sequenceIndex]) != InputCommand.None) {
                        return sequenceIndex + 1;
                    }

                    return sequenceIndex;
                })
                .Where(sequenceIndex => sequenceIndex >= _inputSequence.Length);
            
            ObserveStateUpdate
                .Select(args => args.StateInfo.normalizedTime % 1)
                .Sample(gestureInput)
                .Where(stateTime => stateTime >= _acceptFromStateTime && stateTime < _acceptUntilStateTime)
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    foreach (var hash in outputHashes) {
                        animator.SetTrigger(hash);
                    }
                })
                .AddTo(this);

            ObserveStateEnter
                .CatchIgnoreLog()
                .Subscribe(args => {
                    foreach (var hash in outputHashes) {
                        animator.ResetTrigger(hash);
                    }
                })
                .AddTo(this);
        }
    }
}