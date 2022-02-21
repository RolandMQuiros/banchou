using System;
using System.Collections.Generic;
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
    public class InputGestureToParameters : FSMBehaviour {
        [SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
        private InputCommand[] _inputSequence = null;
        
        [SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
        public float _inputLifetime = 0.1666667f; // Approximately 10 frames
        
        [SerializeField, Tooltip("A command gesture asset. Overrides the Input Sequence and Lifetime if provided.")]
        private PlayerCommandGesture _overrideGesture;

        [SerializeField, Tooltip("Animator conditions that must be filled before a gesture can be accepted")]
        private FSMParameterCondition[] _conditions;

        [SerializeField,
         Tooltip("Whether or not the following times are expressed in normalized state time or in seconds")]
        private bool _inNormalizedTime = true;
        
        [SerializeField, Tooltip("The time after which the command is accepted")]
        private float _acceptFromTime;

        [SerializeField, Tooltip("The time after which the command is no longer accepted")]
        private float _acceptUntilTime = 1f;

        [SerializeField, Tooltip("The the output parameters to set if the gesture was input correctly")]
        private List<ApplyFSMParameter> _output;
        
        [SerializeField, Tooltip("Pause the editor if the gesture is input")]
        private bool _breakOnGesture;
        
        [SerializeField, Tooltip("Pause the editor if the gesture is accepted")]
        private bool _breakOnAccept;

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
                    
                    while (sequenceIndex < _inputSequence.Length &&
                           (unitPair.Current.Command & _inputSequence[sequenceIndex]) != InputCommand.None) {
                        sequenceIndex++;
                    }

                    return sequenceIndex;
                })
                .Where(sequenceIndex => sequenceIndex >= _inputSequence.Length);
            
            // Reset the trigger and entry timestamp
            var enterTime = 0f;
            ObserveStateEnter
                .CatchIgnoreLog()
                .Subscribe(_ => { enterTime = state.GetTime(); })
                .AddTo(this);
            
            // Check which timescale we're using
            IObservable<float> observeGestures;
            if (_inNormalizedTime) {
                observeGestures = ObserveStateUpdate
                    .Select(args => args.StateInfo.normalizedTime % 1);
            } else {
                observeGestures = ObserveStateUpdate
                    .Select(_ => state.GetTime() - enterTime);
            }

            if (_breakOnGesture) {
                gestureInput.Subscribe(_ => Debug.Break())
                    .AddTo(this);
            }
            
            observeGestures
                .Sample(gestureInput)
                .Where(stateTime => stateTime >= _acceptFromTime && stateTime < _acceptUntilTime &&
                                    _conditions.All(condition => condition.Evaluate(animator)))
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    if (_breakOnAccept) {
                        Debug.Break();
                    }
                    _output.ApplyAll(animator);
                })
                .AddTo(this);
        }
    }
}