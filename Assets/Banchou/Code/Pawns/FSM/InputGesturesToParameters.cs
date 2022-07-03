using System;
using System.Linq;
using System.Collections.Generic;
using Banchou.Player;
using UniRx;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Banchou.Pawn.FSM
{
    public class InputGesturesToParameters : PawnFSMBehaviour
    {
        [Serializable]
        private class InputGesture : ISerializationCallbackReceiver {
            [SerializeField, HideInInspector] private string _name;
            
            [SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
            private InputCommand[] _inputSequence = null;

            [SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
            private float _inputLifetime = 0.1666667f; // Approximately 10 frames

            [SerializeField, Tooltip("Animator conditions that must be fulfilled before a gesture can be accepted")]
            private FSMParameterCondition[] _acceptanceConditions;

            [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is accepted")]
            private float _acceptFromTime;

            [SerializeField, Range(0f, 1f),
             Tooltip("The normalized state time after which the command is no longer accepted")]
            private float _acceptUntilTime = 1f;

            [SerializeField, Tooltip("Animator conditions that must be fulfilled before an accepted gesture is applied"),
             FormerlySerializedAs("_conditions")]
            private FSMParameterCondition[] _bufferConditions;

            [SerializeField, Range(0f, 1f),
             Tooltip("The normalized state time after which triggers are set if a command was accepted.")]
            private float _bufferUntilTime;

            [SerializeField, Tooltip("The the output parameters to set if the gesture was input correctly")]
            private List<OutputFSMParameter> _output;

            [SerializeField, Tooltip("Pause the editor if the gesture is input")]
            private bool _breakOnGesture;

            [SerializeField, Tooltip("Pause the editor if the gesture is accepted")]
            private bool _breakOnAccept;

            private InputCommand _commandMask;
            private bool _performed;
            private bool _accepted;
            private int _sequenceIndex;

            private InputCommand _lastCommand = InputCommand.None;
            private float _whenLastInput;

            public InputGesture() { }
            
            public InputGesture(InputGestureToParameters old) {
                _inputSequence = old._inputSequence;
                _inputLifetime = old._inputLifetime;
                _acceptanceConditions = old._acceptanceConditions;
                _acceptFromTime = old._acceptFromTime;
                _acceptUntilTime = old._acceptUntilTime;
                _bufferConditions = old._bufferConditions;
                _bufferUntilTime = old._bufferUntilTime;
                _output = old._output;
                _breakOnGesture = old._breakOnGesture;
                _breakOnAccept = old._breakOnAccept;
            }
            
            public void Initialize() {
                _commandMask = _inputSequence.Aggregate((prev, next) => prev | next);
            }

            public void ProcessCommand(Animator animator, InputCommand command, float when) {
                if ((command & _commandMask) == InputCommand.None) return;
                
                var sequenceStarted = _sequenceIndex > 0;
                var isPreviousCommandTooOld = when - _whenLastInput >= _inputLifetime;

                if (sequenceStarted && isPreviousCommandTooOld) {
                    _sequenceIndex = 0;
                } else {
                    while (_sequenceIndex < _inputSequence.Length &&
                           (command & _inputSequence[_sequenceIndex]) != InputCommand.None) {
                        _sequenceIndex++;
                        _lastCommand = command;
                        _whenLastInput = when;
                    }
                }

                var gestureDetected = _sequenceIndex >= _inputSequence.Length;
                var canPerform = _acceptanceConditions.Length == 0 || _acceptanceConditions.Evaluate(animator);
                _performed = gestureDetected && canPerform;
                
                if (_performed) {
                    _sequenceIndex = 0;
                    if (_breakOnGesture) {
                        Debug.Break();
                    }
                }
            }

            public void Apply(Animator animator, ref FSMUnit fsmUnit) {
                var stateTime = fsmUnit.StateInfo.normalizedTime % 1f;
                _accepted = !_accepted && _performed &&
                                   stateTime >= _acceptFromTime &&
                                   stateTime <= _acceptUntilTime;

                if (_accepted && stateTime >= _bufferUntilTime && _bufferConditions.Evaluate(animator)) {
                    _performed = false;
                    _accepted = false;
                    if (_breakOnAccept) {
                        Debug.Break();
                    }
                    _output.ApplyAll(animator);
                }

                if (fsmUnit.StateEvent == StateEvent.OnExit) {
                    _performed = false;
                    _accepted = false;
                }
            }

            public void OnBeforeSerialize() {
                var oldAcceptFromTime = _acceptFromTime;
                _acceptFromTime = Mathf.Clamp(_acceptFromTime, 0f, _acceptUntilTime);
                _acceptUntilTime = Mathf.Clamp(_acceptUntilTime, oldAcceptFromTime, 1f);
            }

            public void OnAfterDeserialize() {
                _name = string.Join(",", _inputSequence.Select(s => $"[{s.EmojiString()}]"));
            }
        }

        [SerializeField] private InputGesture[] _gestures = new[] { new InputGesture() };
        
        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            ConstructCommon(state, getPawnId);
            
            foreach (var gesture in _gestures) gesture.Initialize();

            state.ObservePawnInputCommands(getPawnId())
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(
                    inputUnit => {
                        var now = State.GetTime();
                        for (int i = 0; i < _gestures.Length; i++) {
                            var gesture = _gestures[i];
                            gesture.ProcessCommand(animator, inputUnit.Command, inputUnit.When);
                        }
                    }
                )
                .AddTo(this);
        }

        private void OnValidate() {
            // Make sure at least one gesture is in the array with the correct default values
            if (_gestures.Length == 0) {
                Array.Resize(ref _gestures, 1);
                _gestures[0] = new InputGesture();
            }
        }

        private void Apply(Animator animator, ref FSMUnit fsmUnit) {
            foreach (var t in _gestures) t.Apply(animator, ref fsmUnit);
        }

        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            Apply(animator, ref fsmUnit);
        }

        [ContextMenu("Migrate Legacy InputGestureToParameters")]
        private void MigrateLegacy() {
            var context = AnimatorController
                .FindStateMachineBehaviourContext(this)?
                .ElementAt(0);
            var owner = context?.animatorObject;

            if (owner is AnimatorState state) {
                var gestures = state.behaviours
                    .Where(b => b is InputGestureToParameters)
                    .Cast<InputGestureToParameters>()
                    .Select(old => new InputGesture(old))
                    .ToArray();

                if (gestures.Length > 0) {
                    _gestures = gestures;
                    state.behaviours = state.behaviours
                        .Where(b => b is not InputGestureToParameters)
                        .ToArray();
                }
            } else if (owner is AnimatorStateMachine stateMachine) {
                var gestures = stateMachine.behaviours
                    .Where(b => b is InputGestureToParameters)
                    .Cast<InputGestureToParameters>()
                    .Select(old => new InputGesture(old))
                    .ToArray();

                if (gestures.Length > 0) {
                    _gestures = gestures;
                    stateMachine.behaviours = stateMachine.behaviours
                        .Where(b => b is not InputGestureToParameters)
                        .ToArray();
                }
            }
        }
    }
}
