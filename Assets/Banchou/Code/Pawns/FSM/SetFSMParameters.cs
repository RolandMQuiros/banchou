using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetFSMParameters : FSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, AtTime = 2, OnExit = 8 }
        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField, Range(0f, 1f)] private float _stateTime;
        [SerializeField] private FSMParameterCondition[] _conditions;
        [SerializeField] private List<OutputFSMParameter> _output;

        private float _timer;
        private bool _appliedAtTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter) && _conditions.Evaluate(animator)) {
                _output.ApplyAll(animator);
            }
            _appliedAtTime = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            var conditionsMet = _conditions.Evaluate(animator);

            if (_onEvent.HasFlag(ApplyEvent.AtTime) && !_appliedAtTime &&
                stateInfo.normalizedTime >= _stateTime && conditionsMet) {
                _output.ApplyAll(animator);
                _appliedAtTime = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit) && _conditions.Evaluate(animator)) {
                _output.ApplyAll(animator);
            }
            _appliedAtTime = false;
        }
    }
}