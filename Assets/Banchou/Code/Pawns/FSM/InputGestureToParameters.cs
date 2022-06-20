using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Banchou.Player;
using UnityEngine.Serialization;

namespace Banchou.Pawn.FSM {
    /*
        Maybe this should be moved/replicated on MonoBehaviour level, so we at least have them all in one
        place to generate command lists from. Then AIs will just need to force trigger names directly instead
        of input commands?

        but also AIs inputting commands kinda rules.
    */
    public class InputGestureToParameters : PawnFSMBehaviour {
        [SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
        private InputCommand[] _inputSequence = null;

        [SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
        private float _inputLifetime = 0.1666667f; // Approximately 10 frames

        [SerializeField, Tooltip("A command gesture asset. Overrides the Input Sequence and Lifetime if provided.")]
        private PlayerCommandGesture _overrideGesture;

        [SerializeField,
         Tooltip("Whether or not times are expressed in normalized state time or in seconds")]
        private bool _inNormalizedTime = true;

        [SerializeField, Tooltip("Animator conditions that must be fulfilled before a gesture can be accepted")]
        private FSMParameterCondition[] _acceptanceConditions;

        [SerializeField, Tooltip("The time after which the command is accepted")]
        private float _acceptFromTime;

        [SerializeField, Tooltip("The time after which the command is no longer accepted")]
        private float _acceptUntilTime = 1f;

        [SerializeField, Tooltip("Animator conditions that must be fulfilled before an accepted gesture is applied"),
         FormerlySerializedAs("_conditions")]
        private FSMParameterCondition[] _bufferConditions;

        [SerializeField, Tooltip("When, in state time, after which triggers are set if a command was accepted.")]
        private float _bufferUntilTime;

        [SerializeField, Tooltip("The the output parameters to set if the gesture was input correctly")]
        private List<ApplyFSMParameter> _output;

        [SerializeField, Tooltip("Pause the editor if the gesture is input")]
        private bool _breakOnGesture;

        [SerializeField, Tooltip("Pause the editor if the gesture is accepted")]
        private bool _breakOnAccept;

        [SerializeField, Tooltip("Pause the editor if this input command is detected")]
        private InputCommand _breakOnCommand;

        private bool _gesturePerformed;
        private bool _gestureAccepted;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            ConstructCommon(state, getPawnId);

            if (_overrideGesture != null) {
                _inputSequence = _overrideGesture.Sequence;
                _inputLifetime = _overrideGesture.Lifetime;
            }

            if (_inputSequence.Length == 0) return;

            var commandMask = _inputSequence.Aggregate((prev, next) => prev | next);

            state.ObservePawnInputCommands(getPawnId())
                .Where(unit => IsStateActive && (unit.Command & commandMask) != InputCommand.None)
                .StartWith((Command: InputCommand.Neutral, When: state.GetTime()))
                .Pairwise()
                .Scan(0, (sequenceIndex, unitPair) => {
                    if ((unitPair.Current.Command & _breakOnCommand) != InputCommand.None) {
                        Debug.Break();
                    }

                    if (sequenceIndex >= _inputSequence.Length) {
                        sequenceIndex = 0;
                    }

                    var sequenceStarted = sequenceIndex > 0;
                    var isPreviousCommandTooOld = unitPair.Current.When - unitPair.Previous.When >= _inputLifetime;

                    if (sequenceStarted && isPreviousCommandTooOld) {
                        return 0;
                    }

                    while (sequenceIndex < _inputSequence.Length &&
                           (unitPair.Current.Command & _inputSequence[sequenceIndex]) != InputCommand.None) {
                        sequenceIndex++;
                    }

                    return sequenceIndex;
                })
                .Where(sequenceIndex => sequenceIndex >= _inputSequence.Length)
                .Where(_ => _acceptanceConditions.Length == 0 ||
                            _acceptanceConditions.All(condition => condition.Evaluate(animator)))
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    if (_breakOnGesture) {
                        Debug.Break();
                    }
                    _gesturePerformed = true;
                })
                .AddTo(this);
        }

        private void Apply(Animator animator, AnimatorStateInfo stateInfo) {
            var stateTime = _inNormalizedTime ? stateInfo.normalizedTime % 1 : StateTime;

            _gestureAccepted = !_gestureAccepted &&
                               _gesturePerformed &&
                               stateTime >= _acceptFromTime &&
                               stateTime <= _acceptUntilTime;

            if (_gestureAccepted && stateTime >= _bufferUntilTime && _bufferConditions.All(c => c.Evaluate(animator))) {
                _gesturePerformed = false;
                _gestureAccepted = false;
                if (_breakOnAccept) {
                    Debug.Break();
                }
                _output.ApplyAll(animator);
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Apply(animator, stateInfo);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            Apply(animator, stateInfo);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            Apply(animator, stateInfo);
            _gesturePerformed = false;
            _gestureAccepted = false;
        }
    }
}